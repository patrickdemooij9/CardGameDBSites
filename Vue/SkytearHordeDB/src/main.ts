import { createApp } from 'vue'
import {
  RequirementType,
  Tab,
  type Character,
  type SizeRequirementConfig,
  type SquadSlot,
  type CardLocation,
  type CardLookup,
  type CharacterImage,
  type ConditionalRequirementConfig,
  type EqualValueRequirementConfig,
  type Filter,
  type NotEqualValueRequirementConfig,
  type SameValueRequirementConfig,
  type Squad,
  type SquadRequirement,
  type UniqueValueRequirementConfig,
  type CardAmount,
  type RequiredCardConfig,
  type ResourceRequirementConfig,
  type SquadGrouping,
  type RichCardAmount,
  DisplaySize,
  type ChildOfRequirementConfig,
  type FixedSquadAmountConfig,
  type DynamicSquadAmountConfig,
  type DeckMutation,
  type ComputedRequirementConfig,
  ComputedType,
  ComputedComparisonType
} from './models/builderModels'
import { ItemOption, type Filter as FilterFilter, type FilterItem } from './models/filterModels'

const beforeUnloadListener = (event: BeforeUnloadEvent) => {
  event.preventDefault()
  return (event.returnValue = '')
}

const app = createApp({
  data() {
    return {
      id: 0,
      typeId: 1,
      name: '',
      description: '',
      isLoggedIn: false,
      requirements: [],
      characters: {},
      squads: [],
      ownedCharacters: {},
      preselectFirstSlot: false,
      hasDynamicSquads: false,
      hasDynamicSlot: false,
      maxDynamicSlots: 0,

      selectedCharacter: undefined,
      selectedCharacterImage: undefined,
      selectedSquad: undefined,
      selectedSlot: undefined,

      dataString: '',
      currentTab: Tab.Squad,

      markdownPreview: false,
      markdownPreviewText: '',

      filteredCardIds: null,
      search: '',
      loadingCards: false,
      additionalFilterRequirements: [],
      filters: [], //Todo: This should probably be a new component,
      showFilters: false,
      openFilter: '',
      dirty: false,
      publish: false,
      collectionMode: false,
      dynamicSlots: [],
      needsIndexOnCounts: false,

      builderElement: undefined
    }
  },
  mounted() {
    this.builderElement = document.getElementById('builder')
    if (!this.builderElement) {
      return
    }
    const initModel = JSON.parse(this.builderElement.dataset.builder!.toString())

    this.id = initModel.id
    this.typeId = initModel.typeId
    this.name = initModel.name
    this.description = initModel.description
    this.isLoggedIn = initModel.isLoggedIn
    this.requirements = initModel.requirements
    this.preselectFirstSlot = initModel.preselectFirstSlot
    this.hasDynamicSquads = initModel.hasDynamicSquads
    this.hasDynamicSlot = initModel.hasDynamicSlot
    this.ownedCharacters = initModel.ownedCharacters
    this.maxDynamicSlots = initModel.maxDynamicSlots

    initModel.allCharacters.forEach((c: Character) => {
      this.characters[c.id] = c
    })
    this.squads = initModel.squads

    this.init()
    this.builderElement.classList.remove('hidden')
  },
  methods: {
    init() {
      this.squads.forEach((squad: Squad) => {
        if (
          squad.requirements.some((requirement) => this.requirementNeedsIndexOnCounts(requirement))
        ) {
          this.needsIndexOnCounts = true
        }

        squad.slots.forEach((slot) => {
          this.initSlot(squad, slot)
        })
      })

      Array.from(document.getElementsByClassName('filter')).forEach((filterElem) => {
        this.registerFilter(filterElem)
      })

      this.indexCharacters()

      if (this.preselectFirstSlot) {
        let foundSlot = false
        this.squads.forEach((squad: Squad) => {
          if (foundSlot) {
            return
          }

          const nonFilledSlot = squad.slots.find((s) => !this.isSlotFilled(s))
          if (nonFilledSlot !== undefined) {
            this.clickSlot(squad, nonFilledSlot, false)
            foundSlot = true
            return
          }
        })

        if (!foundSlot) {
          const allSlots: SquadSlot[] = this.squads.flatMap((s: Squad) => s.slots)
          let slotToClick = allSlots.find((s) => !this.isSlotFilled(s))
          if (!slotToClick) {
            slotToClick = allSlots[allSlots.length - 1]
          }
          const lastSquad = this.squads[this.squads.length - 1]
          this.clickSlot(lastSquad, lastSquad.slots[lastSquad.slots.length - 1], false)
        }
      }
    },

    initSlot(squad: Squad, slot: SquadSlot) {
      let cards = this.getCardsBySlot(slot) as CardAmount[]
      if (!cards) {
        cards = []
      }

      cards.forEach((cardAmount) => {
        const card: Character = this.getCardById(cardAmount.id)
        card.presentInSlot = { squad, slot }

        this.addRequirementsByCharacter(card, squad)

        if (card.mutations) {
          this.performMutations(card.mutations, squad, slot, true)
        }

        cardAmount.children.forEach((childSlot) => {
          this.initSlot(undefined, childSlot)
          this.dynamicSlots.push(childSlot)
        })
      })
    },

    indexCharacters() {
      this.getCards().forEach((character: Character) => {
        this.indexCharacter(character)
      })
    },

    indexCharacter(character: Character) {
      character.validLocations = []

      if (character.presentInSlot) {
        character.validLocations.push({
          squad: character.presentInSlot.squad,
          slot: character.presentInSlot.slot,
          isAllowed:
            !this.isSlotFilled(character.presentInSlot.slot) &&
            (!this.needsIndexOnCounts ||
            this.isAllowedForSquad(character.presentInSlot.squad, character))
        })
        return
      }

      this.dynamicSlots.forEach((slot: SquadSlot) => {
        if (!this.isAllowedForSlot(slot, character)) {
          return
        }
        character.validLocations.push({
          squad: undefined,
          slot: slot,
          isAllowed: !this.isSlotFilled(slot)
        })
      })

      if (!this.isAllowedForTeam(character)) {
        return
      }

      this.squads.forEach((squad: Squad) => {
        if (!this.isAllowedForSquad(squad, character)) {
          return
        }

        squad.slots.forEach((slot) => {
          const isSlotFilled = this.isSlotFilled(slot)
          const isInSlot = (this.getCardsBySlot(slot) as CardAmount[]).some(
            (card) => card.id === character.id
          )

          if (!isInSlot) {
            if (!this.isAllowedForSlot(slot, character)) {
              return
            }
          }

          character.validLocations.push({
            squad: squad,
            slot: slot,
            isAllowed: !isSlotFilled || isInSlot
          })
        })
      })
    },

    addToSquad(squad: Squad, slot: SquadSlot, character: Character, allowRemoval: boolean = true) {
      this.closeModal(true)

      let cardAmount: CardAmount = this.getCardsBySlot(slot).find(
        (card: CardAmount) => card.id === character.id
      )

      if (cardAmount) {
        if (cardAmount.amount >= this.getAllowedSizeForSlot(character)) {
          return
        }

        cardAmount.amount++
        if (this.needsIndexOnCounts) {
          this.indexCharacters()
        }
      } else {
        const childSlots: SquadSlot[] =
          character.maxChildren == 0 ? [] : [this.createChildSlot(character)] //TODO: Find a better solution for this later on

        cardAmount = {
          id: character.id,
          amount: 1,
          allowRemoval: allowRemoval,
          children: childSlots
        }

        childSlots.forEach((slot) => {
          this.dynamicSlots.push(slot)
        })

        //slot.cardIds.push(cardAmount);
        character.presentInSlot = { squad, slot: slot }

        if (!this.preselectFirstSlot && this.isSlotFilled(slot) && this.selectedSlot) {
          this.selectedSquad = undefined
          this.selectedSlot = undefined
          this.currentTab = Tab.Squad
        }

        const groupToAddTo = slot.cardGroups.find(
          (group: SquadGrouping) =>
            group.requirements.length === 0 ||
            group.requirements.every((r) => this.checkRequirement(r, [], character))
        )
        if (!groupToAddTo) {
          console.error('No group found for ' + cardAmount.id)
          return
        }
        groupToAddTo.cardIds.push(cardAmount)

        if (character.mutations) {
          this.performMutations(character.mutations, squad, slot, true)
        }

        this.addRequirementsByCharacter(character, squad)
        this.indexCharacters()
      }

      if (!this.dirty) {
        this.dirty = true
        addEventListener('beforeunload', beforeUnloadListener, { capture: true })
      }

      /*if (this.isSlotFilled(characterSlot)) {
          var currentSlotIndex = squad.slots.indexOf(characterSlot);
          if (currentSlotIndex === squad.slots.length - 1) { return; }

          this.clickSlot(squad, squad.slots[currentSlotIndex + 1]);
      }*/
    },

    createChildSlot(character: Character): SquadSlot {
      return {
        id: -1,
        label: 'Battle Tactics',
        requirements: [
          {
            type: RequirementType.ChildOf,
            config: {
              parentId: character.id
            }
          }
        ],
        cardGroups: [
          {
            displayName: '',
            sortBy: '',
            cardIds: [],
            requirements: []
          }
        ],
        minCards: 0,
        maxCardAmount: {
          type: 'fixed',
          config: {
            amount: character.maxChildren
          }
        },
        displaySize: DisplaySize.Small,
        disableRemoval: false,
        numberMode: false,
        showIfTargetSlotIsFilled: undefined,
        additionalFilterRequirements: []
      }
    },

    removeCharacterFromSquad(character: Character) {
      const slot = character.presentInSlot?.slot
      if (!slot) {
        return
      }

      const wasFilled = this.isSlotFilled(slot)

      const cardAmount: CardAmount = this.getCardsBySlot(slot).find(
        (card: CardAmount) => card.id === character.id
      )
      if (!cardAmount) {
        return
      }

      if (!this.dirty) {
        this.dirty = true
        addEventListener('beforeunload', beforeUnloadListener, { capture: true })
      }

      if (cardAmount.amount > 1) {
        cardAmount.amount--
        if (wasFilled || this.needsIndexOnCounts) {
          this.indexCharacters()
        }

        return
      }

      this.closeModal(true)

      const lookupResult: CardLookup = this.getSlotByCharacter(character)
      character.teamRequirements.forEach((tr) => {
        const index = this.requirements.indexOf(tr, 0)
        if (index > -1) {
          this.requirements.splice(index, 1)
        }
      })
      character.squadRequirements.forEach((sr) => {
        const index = lookupResult.squad.requirements.indexOf(sr, 0)
        if (index > -1) {
          lookupResult.squad.requirements.splice(index, 1)
        }
      })
      character.slotRequirements.forEach((sr) => {
        const slot = lookupResult.squad.slots[sr.slotId]
        sr.requirements.forEach((r) => {
          this.checkAdditionalRequirementAction(r, lookupResult.squad, slot, true)
          const index = slot.requirements.indexOf(r, 0)
          if (index > -1) {
            slot.requirements.splice(index, 1)
          }
        })
      })

      if (cardAmount.children.length > 0) {
        cardAmount.children.forEach((childSlot) => {
          this.getCardsBySlot(childSlot).forEach((childCard: CardAmount) => {
            this.getCardById(childCard.id).presentInSlot = undefined
          })
          this.dynamicSlots.splice(this.dynamicSlots.indexOf(childSlot), 1)
        })
        cardAmount.children = []
      }

      const cardGroup = this.getGroupByCard(slot, character.id) as SquadGrouping
      cardGroup.cardIds.splice(cardGroup.cardIds.indexOf(cardAmount), 1)
      character.presentInSlot = undefined

      if (character.mutations) {
        this.performMutations(character.mutations, lookupResult.squad, slot, false)
      }

      this.indexCharacters()
    },

    removeFromSquad(characterSlot: SquadSlot) {
      this.closeModal(true)
      this.getCardsBySlot(characterSlot).forEach((cardId: CardAmount) => {
        this.removeCharacterFromSquad(this.getCardById(cardId.id))
      })
    },

    performMutations(mutations: DeckMutation[], squad: Squad, slot: SquadSlot, apply: boolean) {
      mutations.forEach((item) => {
        const slotToChange = item.slotId === 0 ? slot : squad.slots[item.slotId]
        if (item.alias === 'minCards') {
          slotToChange.minCards += apply ? item.change : -item.change
        }
      })
    },

    addRequirementsByCharacter(character: Character, squad: Squad) {
      character.teamRequirements.forEach((tr) => {
        this.requirements.push(tr)
      })
      character.squadRequirements.forEach((sr) => {
        squad.requirements.push(sr)
      })
      character.slotRequirements.forEach((sr) => {
        const slot = squad.slots[sr.slotId]

        sr.requirements.forEach((r) => {
          this.checkAdditionalRequirementAction(r, squad, slot, false)
        })
        slot.requirements.push(...sr.requirements)
      })
    },

    checkAdditionalRequirementAction(
      requirement: SquadRequirement,
      squad: Squad,
      slot: SquadSlot,
      remove: boolean
    ) {
      if (requirement.type === RequirementType.RequiredCard) {
        const config = requirement.config as RequiredCardConfig
        const requiredCard = this.getCardById(config.requiredCardId)

        if (!requiredCard) {
          return
        }

        if (remove) {
          this.removeCharacterFromSquad(requiredCard)
        } else {
          const existing = this.getCardsBySlot(slot).find(
            (it: CardAmount) => it.id === config.requiredCardId
          )
          if (existing) {
            existing.allowRemoval = false
          } else {
            this.addToSquad(squad, slot, requiredCard, false)
          }
        }
      }
    },

    clickSlot(squad: Squad, slot: SquadSlot, switchTab: boolean) {
      const element = document.getElementById('allCharacters')

      if (element) {
        element.scrollIntoView()
      }

      window.dispatchEvent(new CustomEvent('ResetFilters'))

      if (switchTab) {
        this.currentTab = Tab.Cards
      }
      this.selectedSquad = squad
      this.selectedSlot = slot

      this.additionalFilterRequirements = []
      if (slot.additionalFilterRequirements) {
        this.additionalFilterRequirements = [...slot.additionalFilterRequirements]
      }
    },

    addSlotAdditionalFilterRequirements() {
      if (!this.selectedSlot || !this.selectedSlot.additionalFilterRequirements) {
        return
      }

      this.additionalFilterRequirements = [...this.selectedSlot.additionalFilterRequirements]
    },

    selectCharacter(character: Character) {
      this.selectedCharacter = character
      this.selectedCharacterImage = character.images.find((item) => item.isPrimaryImage)

      if (!window.history.state || !window.history.state.modal) {
        window.history.pushState({ modal: true }, '', null) // So we can block the back button.
      }
      window.addEventListener('popstate', this.handleModalBackButton)

      document.querySelector('body')!.classList.add('modal-open')
    },

    selectImage(image: CharacterImage) {
      this.selectedCharacterImage = image
    },

    closeModal(withHistoryBack: boolean) {
      if (!this.selectedCharacter) {
        return
      }

      this.selectedCharacter = undefined
      this.selectedCharacterImage = undefined

      window.removeEventListener('popstate', this.handleModalBackButton)
      if (withHistoryBack) {
        window.history.back()
      }

      document.querySelector('body')!.classList.remove('modal-open')
    },

    handleModalBackButton() {
      if (this.selectCharacter) {
        this.closeModal(false)
      }
    },

    clickTab(tab: Tab) {
      this.currentTab = tab

      if (!this.preselectFirstSlot) {
        this.selectedSlot = undefined
        this.selectedSquad = undefined
      }
    },

    async toggleMarkdownPreview() {
      if (!this.markdownPreview) {
        const response = await fetch('/umbraco/api/markdown/preview', {
          method: 'POST',
          body: JSON.stringify({
            markdown: this.description
          }),
          headers: {
            'Content-type': 'application/json; charset=UTF-8'
          }
        })
        const body = await response.text()
        this.markdownPreviewText = body
      }

      this.markdownPreview = !this.markdownPreview
    },

    getCharacterSpotValidForSquad(squad: Squad, character: Character): SquadSlot | undefined {
      return squad.slots.find((slot) => {
        if (
          !this.isSlotFilled(slot) &&
          !this.isSlotFillForCard(slot, character) &&
          character.validLocations.some((l) => l.slot === slot && l.squad === squad)
        ) {
          return true
        }
      })
    },

    getValidLocations(character: Character) {
      return character.validLocations
    },

    getGroupByCard(slot: SquadSlot, cardId: number) {
      return slot.cardGroups.find((group) => group.cardIds.some((c) => c.id === cardId))
    },

    getOrderedGroupCards(group: SquadGrouping): RichCardAmount[] {
      const cards = group.cardIds.map((cardId) => ({
        card: this.getCardById(cardId.id),
        amount: cardId.amount,
        allowRemoval: cardId.allowRemoval,
        children: cardId.children ?? []
      }))

      if (group.sortBy) {
        return cards.sort(
          (item1, item2) =>
            this.getAbilityValueByType(item1.card, group.sortBy) -
            this.getAbilityValueByType(item2.card, group.sortBy)
        )
      }
      return cards
    },

    getCardsBySlot(slot: SquadSlot): CardAmount[] {
      return slot.cardGroups.flatMap((group) => group.cardIds)
    },

    isLocationVisible(location: CardLocation): boolean {
      if (this.selectedSlot) {
        return location.slot === this.selectedSlot
      }
      return true
    },

    isLocationAllowed(location: CardLocation): boolean {
      return location.isAllowed && this.isLocationVisible(location)
    },

    isPresentInSlot(location: CardLocation, character: Character): boolean {
      if (!character.presentInSlot) {
        return false
      }
      return this.getCardsBySlot(location.slot).some((it: CardAmount) => it.id === character.id)
    },

    isAllowedForAll(squad: Squad, slot: SquadSlot, character: Character): boolean {
      if (!this.isAllowedForTeam(character)) {
        return false
      }
      if (!this.isAllowedForSquad(squad, character)) {
        return false
      }
      return this.isAllowedForSlot(slot, character, [])
    },

    isAllowedForTeam(character: Character): boolean {
      let valid = true

      const allCharactersInSquads: CardAmount[] = this.squads
        .flatMap((s: Squad) => s.slots)
        .flatMap((s: SquadSlot) => this.getCardsBySlot(s))
      //Check the team requirements
      this.requirements.forEach((requirement: SquadRequirement) => {
        if (!this.checkRequirement(requirement, allCharactersInSquads, character)) {
          valid = false
        }
      })

      if (!valid) {
        return false
      }

      //Check specific character team requirements
      if (character.teamRequirements.length > 0) {
        character.teamRequirements.forEach((requirement) => {
          if (!this.checkRequirement(requirement, allCharactersInSquads, character)) {
            valid = false
          }
        })
      }
      return valid
    },

    isAllowedForSquad(squad: Squad, character: Character): boolean {
      let valid = true

      //Check the squad requirements
      const squadCharacters: CardAmount[] = squad.slots.flatMap((it) => this.getCardsBySlot(it))
      squad.requirements.forEach((requirement) => {
        if (!this.checkRequirement(requirement, squadCharacters, character)) {
          valid = false
        }
      })

      if (!valid) {
        return false
      }

      //Check specific character squad requirements
      if (character.squadRequirements.length > 0) {
        character.squadRequirements.forEach((requirement) => {
          if (!this.checkRequirement(requirement, squadCharacters, character)) {
            valid = false
          }
        })
      }
      return valid
    },

    isAllowedForSlot(slot: SquadSlot, character: Character): boolean {
      let valid = true

      const slotCharacters: CardAmount[] = this.getCardsBySlot(slot)

      //Check the slot requirement
      slot.requirements.forEach((requirement) => {
        if (!this.checkRequirement(requirement, slotCharacters, character)) {
          valid = false
        }
      })

      if (!valid) {
        return false
      }

      if (character.slotRequirements.length > 0) {
        if (this.getCards().some((c: Character) => !this.checkTargetRequirements(character, c))) {
          valid = false
        }
      }

      return valid
    },

    checkTargetRequirements(character: Character, characterToCheck: Character) {
      if (!characterToCheck.presentInSlot) {
        return true
      }
      const requirements = character.slotRequirements
        .filter((r) => r.slotId === characterToCheck.presentInSlot!.slot.id)
        .flatMap((r) => r.requirements)
      if (requirements.length === 0) {
        return true
      }

      return requirements.every((r) => this.checkRequirement(r, [], characterToCheck))
    },

    isSlotFilled(slot: SquadSlot): boolean {
      const canBeInfinite = this.slotAmountCanBeInfinite(slot)
      const maxCards = this.getMaxSlotAmount(slot)
      if (canBeInfinite && maxCards === 0) {
        return false
      }
      return this.getSlotAmount(slot) >= maxCards
    },

    isSlotFillForCard(slot: SquadSlot, character: Character): boolean {
      const cardAmount = this.getCardsBySlot(slot).find((it: CardAmount) => it.id === character.id)
      if (!cardAmount) return false
      return cardAmount.amount >= this.getAllowedSizeForSlot(character)
    },

    isCardAllowedToRemove(slot: SquadSlot, character: Character): boolean {
      if (slot.disableRemoval) {
        return false
      }

      const cardAmount = this.getCardsBySlot(slot).find((it: CardAmount) => it.id === character.id)
      if (!cardAmount) return false
      return cardAmount.allowRemoval
    },

    isAParent(character: Character): boolean {
      return character.allowedChildren.length > 0
    },

    getCharacterAmount(slot: SquadSlot, character: Character): number {
      const cardAmount = this.getCardsBySlot(slot).find((it: CardAmount) => it.id === character.id)
      if (!cardAmount) return 0
      return cardAmount.amount
    },

    getSquadAmount(squad: Squad): number {
      return squad.slots.reduce((sum, slot) => sum + this.getSlotAmount(slot), 0)
    },

    getMaxSquadAmount(squad: Squad): number {
      return squad.slots.reduce((sum, slot) => {
        const maxCards = this.getMaxSlotAmount(slot)
        return sum + (maxCards === 0 ? slot.minCards : maxCards)
      }, 0)
    },

    getDeckAmount(): number {
      return this.squads.reduce((sum: number, squad: Squad) => sum + this.getSquadAmount(squad), 0)
    },

    getMaxDeckAmount(): number {
      return this.squads.reduce(
        (sum: number, squad: Squad) => sum + this.getMaxSquadAmount(squad),
        0
      )
    },

    getSlotAmount(slot: SquadSlot): number {
      return this.getCardsBySlot(slot).reduce(
        (sum: number, value: CardAmount) => sum + value.amount,
        0
      )
    },

    getAvailableSquads(): Squad[] {
      if (!this.hasDynamicSquads) {
        return this.squads
      }

      const squads: Squad[] = []
      this.squads.forEach((squad: Squad) => {
        if (this.getSquadAmount(squad) > 0 || this.getMaxSquadAmount(squad) !== 0) {
          squads.push(squad)
        }
      })

      return squads
    },

    getAvailableSlots(squad: Squad): SquadSlot[] {
      if (!this.hasDynamicSlot) {
        return squad.slots
      }

      const filledSlots = squad.slots.filter((it) => this.isSlotFilled(it)).map((it) => it.id)
      return squad.slots.filter((it) => {
        const result =
          it.showIfTargetSlotIsFilled === null || filledSlots.includes(it.showIfTargetSlotIsFilled!)
        return result
      })
    },

    getSlotsByCard(cardAmount: RichCardAmount): SquadSlot[] {
      const card = cardAmount.card
      if (!card || card.allowedChildren.length === 0) {
        return []
      }
      if (!this.hasDynamicSlot) {
        return cardAmount.children
      }

      const filledSlots = cardAmount.children
        .filter((it) => this.isSlotFilled(it))
        .map((it) => it.id)
      return cardAmount.children.filter((it) => {
        const result =
          it.showIfTargetSlotIsFilled === null || filledSlots.includes(it.showIfTargetSlotIsFilled!)
        return result
      })
    },

    getFilteredCharacters(): Character[] {
      if (!this.selectedSquad) return []

      return this.getCards().filter((character: Character) => {
        if (
          character.validLocations.some(
            (l) => l.slot === this.selectedSlot && l.squad === this.selectedSquad
          )
        ) {
          return true
        }
        return false
      })
    },

    isShownInOverview(character: Character): boolean {
      if (!this.selectedSquad || !this.selectedSlot) return true

      const valid =
        character.validLocations.find(
          (l) =>
            (l.squad === undefined || l.squad === this.selectedSquad) &&
            l.slot === this.selectedSlot
        ) !== undefined

      if (this.additionalFilterRequirements && valid) {
        //TODO: We should do this with the index already
        const selectedMainCharacters = this.selectedSquad.slots
          .filter((slot: SquadSlot) => slot.id !== 2)
          .flatMap((slot: SquadSlot) => this.getCardsBySlot(slot))
        return this.additionalFilterRequirements.every((r: SquadRequirement) =>
          this.checkRequirement(r, selectedMainCharacters, character)
        )
      }
      return valid
    },

    getSlotByCharacter(character: Character): CardLookup | undefined {
      let lookup: CardLookup | undefined = undefined

      this.squads.forEach((squad: Squad) => {
        const foundSlot = squad.slots.find((slot) =>
          this.getCardsBySlot(slot).some((it: CardAmount) => it.id === character.id)
        )
        if (foundSlot) {
          lookup = {
            slot: foundSlot,
            squad: squad
          }
        }
      })
      return lookup
    },

    getAllowedSizeForSlot(character: Character): number {
      const maxSize = this.getMaxSizeForSlot(character)
      if (!this.collectionMode) {
        return maxSize
      }
      let ownedAmount = 0
      if (Object.keys(this.ownedCharacters).includes(character.id.toString())) {
        ownedAmount = this.ownedCharacters[character.id.toString()]
      }

      return Math.min(maxSize, ownedAmount)
    },

    getMaxSizeForSlot(character: Character): number {
      const values = this.getAbilityValueByType(character, 'Amount')
      if (!values || values.length === 0) {
        return 1
      }
      return parseInt(values[0])
    },

    showDynamicSlot(slot: SquadSlot): boolean {
      const hasCards = this.getCardsBySlot(slot).length > 0
      if (hasCards) {
        return true
      }

      const slotsWithData = this.dynamicSlots.filter(
        (slot: SquadSlot) => this.getCardsBySlot(slot).length > 0
      )
      return slotsWithData < this.maxDynamicSlots
    },

    isPresentInSquad(character: Character): boolean {
      return this.getSlotByCharacter(character)
    },

    getCards(): Character[] {
      return Object.values<Character>(this.characters)
    },

    getCardById(id: number | string): Character | undefined {
      if (typeof id === 'string') {
        id = parseInt(id)
      }

      return this.characters[id]
    },

    getAbilityValueByType(character: Character, type: string): string[] {
      return character.abilities.find((item) => item.type === type)?.values ?? []
    },

    getAbilityDisplayByType(character: Character, type: string): string {
      return character?.abilities.find((item) => item.type === type)?.displayValue ?? ''
    },

    hasAbility(character: Character, type: string) {
      const ability = character.abilities.find((item) => item.type === type)
      return !!ability
    },

    hasAbilityValue(character: Character, type: string, value: string) {
      const ability = character.abilities.find((item) => item.type === type)
      if (!ability) {
        return false
      }

      return ability.values.includes(value)
    },

    hasMainImage(character: Character) {
      return character.images.some((item) => item.isPrimaryImage)
    },

    getMainImageUrl(character: Character) {
      return character.images.find((item) => item.isPrimaryImage)?.imageUrl ?? '/'
    },

    getDisplayClassesForItem(slot: SquadSlot, item: any) {
      const classes = []
      if (slot.displaySize === 'Small') {
        classes.push('py-1')
      }

      if (item && !this.hasEnoughInCollection(item.card.id, item.amount)) {
        classes.push('border-red-300')
      } else {
        classes.push('border-gray-300')
      }
      return classes
    },

    slotAmountCanBeInfinite(slot: SquadSlot) {
      return slot.maxCardAmount.type === 'fixed'
    },

    getMaxSlotAmount(slot: SquadSlot) {
      if (slot.maxCardAmount.type === 'fixed') {
        const fixedConfig = slot.maxCardAmount.config as FixedSquadAmountConfig
        return fixedConfig.amount
      }

      const dynamicAmount = slot.maxCardAmount.config as DynamicSquadAmountConfig
      const allCharactersInSquads: Character[] = this.squads
        .flatMap((s: Squad) => s.slots)
        .flatMap((s: SquadSlot) => this.getCardsBySlot(s))
        .map((it: CardAmount) => this.getCardById(it.id));

      return allCharactersInSquads.reduce((sum, char) => {
        if (dynamicAmount.requirements.every((req) => this.checkRequirement(req, [char], char))) {
          return sum + 1
        }
        return sum
      }, 0)
    },

    pointsLeft(squad: Squad) {
      const charactersInSquad = squad.slots
        .flatMap((it) => this.getCardsBySlot(it))
        .map((it) => this.getCardById(it.id))
      let points = 0

      charactersInSquad.forEach((character) => {
        const pointsToAdd: string[] = this.getAbilityValueByType(character, 'Points')
        if (pointsToAdd.length > 0) {
          points += parseInt(pointsToAdd[0])
        }
      })
      return points
    },

    getViewStateForTab(tab: Tab) {
      if (this.currentTab === tab) {
        return {}
      }

      return {
        'md:block': this.currentTab === Tab.Squad,
        hidden: true
      }
    },

    removeAdditionalFilter(filter: SquadRequirement) {
      const index = this.additionalFilterRequirements.indexOf(filter)
      if (index > -1) {
        this.additionalFilterRequirements.splice(index, 1)
      }
    },

    checkRequirement(
      requirementConfig: SquadRequirement,
      characters: CardAmount[],
      character: Character
    ): boolean {
      const checkCharacters = [...characters.map((item) => this.getCardById(item.id))]
      if (character) {
        checkCharacters.push(character)
        const existingCharacter = characters.find((item) => item.id === character.id)
        let amount = 1
        if (existingCharacter) {
          characters.splice(characters.indexOf(existingCharacter), 1)
          amount += existingCharacter.amount
        }
        characters.push({
          id: character.id,
          amount: amount,
          allowRemoval: false,
          children: []
        })
      }
      if (requirementConfig.type === RequirementType.UniqueValue) {
        return this.checkUniqueValueRequirement(
          requirementConfig.config as SameValueRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.SameValue) {
        return this.checkSameValueRequirement(
          requirementConfig.config as UniqueValueRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.NotEqualValue) {
        return this.checkNotEqualValueRequirement(
          requirementConfig.config as NotEqualValueRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.EqualValue) {
        return this.checkEqualValueRequirement(
          requirementConfig.config as EqualValueRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.Size) {
        return this.checkSizeRequirement(
          requirementConfig.config as SizeRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.Conditional) {
        return this.checkConditionalRequirement(
          requirementConfig.config as ConditionalRequirementConfig,
          characters
        )
      } else if (requirementConfig.type === RequirementType.RequiredCard) {
        return true
        //Todo: fix?
        // This is currently not used as it will just stop any card from being chosen.
        //return this.checkRequiredCardRequirement(requirementConfig.config as RequiredCardConfig, checkCharacters);
      } else if (requirementConfig.type === RequirementType.Resource) {
        return this.checkResourceRequirement(
          requirementConfig.config as ResourceRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.ChildOf) {
        return this.checkChildOfRequirement(
          requirementConfig.config as ChildOfRequirementConfig,
          checkCharacters
        )
      } else if (requirementConfig.type === RequirementType.Computed) {
        return this.checkComputedRequirement(
          requirementConfig.config as ComputedRequirementConfig,
          characters
        )
      }
      console.error('Requirement not found!')
      return false
    },

    requirementNeedsReindex(requirementConfig: SquadRequirement) {
      const requirementsThatNeedReindex = [
        RequirementType.UniqueValue,
        RequirementType.SameValue,
        RequirementType.Size,
        RequirementType.Conditional,
        RequirementType.Resource,
        RequirementType.Computed
      ]

      return requirementsThatNeedReindex.includes(requirementConfig.type as RequirementType)
    },

    requirementNeedsIndexOnCounts(requirementConfig: SquadRequirement) {
      const requirementNeedsIndexOnCounts = [RequirementType.Computed]

      return requirementNeedsIndexOnCounts.includes(requirementConfig.type as RequirementType)
    },

    requirementToFilter(requirementConfig: SquadRequirement): Filter[] | undefined {
      if (requirementConfig.type === RequirementType.EqualValue) {
        const filters: Filter[] = []
        const equalValueConfig = requirementConfig.config as EqualValueRequirementConfig
        equalValueConfig.values.forEach((v) => {
          filters.push({
            key: equalValueConfig.ability,
            value: v,
            excluded: false
          })
        })
        return filters
      }
      if (requirementConfig.type === RequirementType.NotEqualValue) {
        const filters: Filter[] = []
        const equalValueConfig = requirementConfig.config as NotEqualValueRequirementConfig
        equalValueConfig.values.forEach((v) => {
          filters.push({
            key: equalValueConfig.ability,
            value: v,
            excluded: true
          })
        })
        return filters
      }
      return undefined
    },

    hasEnoughPoints(squad: Squad): Boolean {
      return this.pointsLeft(squad) >= 0
    },

    isValidForSave(): boolean {
      return true
    },

    isValidDeck(): boolean {
      let valid = true
      this.squads.forEach((squad: Squad) => {
        if (
          squad.slots.find((slot) => {
            const maxAmount = this.getMaxSlotAmount(slot)
            const canBeInfinite = this.slotAmountCanBeInfinite(slot)
            return (!canBeInfinite || maxAmount > 0) && this.getSlotAmount(slot) > maxAmount
          })
        ) {
          valid = false
          return
        }
        if (
          squad.slots.find((slot) => slot.minCards > 0 && this.getSlotAmount(slot) < slot.minCards)
        ) {
          valid = false
          return
        }
        if (!this.hasEnoughPoints(squad)) {
          valid = false
          return
        }
      })
      let filledSlots = 0
      this.dynamicSlots.forEach((slot: SquadSlot) => {
        const slotCardAmount = this.getSlotAmount(slot)
        if (slotCardAmount === 0) {
          return
        }

        const maxAmount = this.getMaxSlotAmount(slot)
        if (maxAmount >= slotCardAmount && slot.minCards <= slotCardAmount) {
          filledSlots++
        } else {
          valid = false
          return
        }
      })
      if (filledSlots > this.maxDynamicSlots) {
        return false
      }
      return valid
    },

    submitForm(publish: boolean) {
      if (this.dirty) {
        removeEventListener('beforeunload', beforeUnloadListener, { capture: true })
      }

      this.publish = publish
      this.dataString = JSON.stringify({
        id: this.id,
        typeId: this.typeId,
        name: this.name,
        description: this.description,
        squads: this.squads.map((item: Squad) => ({
          id: item.id,
          slots: item.slots.map((slot) => ({
            id: slot.id,
            cards: this.getCardsBySlot(slot).map((key: CardAmount) => ({
              cardId: key.id,
              amount: key.amount,
              children: key.children.flatMap((childSlot) =>
                this.getCardsBySlot(childSlot).map((card: CardAmount) => card.id)
              )
            }))
          }))
        }))
      })

      this.$nextTick(() => {
        this.builderElement.submit()
      })
    },

    checkUniqueValueRequirement(
      config: UniqueValueRequirementConfig,
      characters: Character[]
    ): boolean {
      const buffer: string[] = []

      for (let i = 0; i < characters.length; i++) {
        const character = characters[i]

        const values = character.abilities.find((item) => item.type === config.ability)?.values
        if (values) {
          if (buffer.some((item) => values.includes(item))) {
            return false
          }

          values.forEach((value) => {
            buffer.push(value)
          })
        }
      }
      return true
    },

    checkSameValueRequirement(
      config: SameValueRequirementConfig,
      characters: Character[]
    ): boolean {
      let buffer: string[] | undefined = undefined

      for (let i = 0; i < characters.length; i++) {
        const character = characters[i]

        const values = character.abilities.find((item) => item.type === config.ability)?.values
        if (!values) {
          return false
        }

        if (!buffer) {
          buffer = [...values]
          continue
        }

        const differentValues = buffer.filter((item) => !values.includes(item))
        differentValues.forEach((value) => {
          const index = buffer!.indexOf(value)
          buffer!.splice(index, 1)
        })

        if (buffer.length === 0) {
          return false
        }
      }
      return true
    },

    checkNotEqualValueRequirement(
      config: NotEqualValueRequirementConfig,
      characters: Character[]
    ): boolean {
      return (
        characters.find((character) => {
          const characterValues = character.abilities.find((item) => item.type === config.ability)
            ?.values
          if (characterValues && characterValues.some((v) => config.values.includes(v))) {
            return true
          }
          return false
        }) === undefined
      )
    },

    checkEqualValueRequirement(
      config: EqualValueRequirementConfig,
      characters: Character[]
    ): boolean {
      return (
        characters.find((character) => {
          const characterValues = character.abilities.find((item) => item.type === config.ability)
            ?.values
          if (!characterValues || !characterValues.some((v) => config.values.includes(v))) {
            return true
          }
          return false
        }) === undefined
      )
    },

    checkSizeRequirement(config: SizeRequirementConfig, characters: Character[]): boolean {
      const size = characters.length
      if (config.min && size < config.min) {
        return false
      }
      if (config.max && size > config.max) {
        return false
      }
      return true
    },

    checkConditionalRequirement(
      config: ConditionalRequirementConfig,
      characters: CardAmount[]
    ): boolean {
      // This might be changed later on. For conditional, we only get the characters that match the requirements and then check those
      // But this might be a bit overkill if we just want to check single characters.

      const charactersToCheck = characters.filter((c) =>
        config.condition.every((config) =>
          this.checkRequirement(config, [], this.getCardById(c.id))
        )
      )
      if (charactersToCheck.length > 0) {
        return config.requirements.every((config) =>
          this.checkRequirement(config, charactersToCheck)
        )
      }
      return true
    },

    checkRequiredCardRequirement(config: RequiredCardConfig, character: Character[]): boolean {
      return character.some((c) => c.id === config.requiredCardId)
    },

    checkResourceRequirement(config: ResourceRequirementConfig, characters: Character[]): boolean {
      const mainCards = characters.filter((c) =>
        config.mainCardsCondition.every((config) => this.checkRequirement(config, [], c))
      )
      if (mainCards.length === 0 || (config.mainAbilityMaxSize > 0 && mainCards.length < config.mainAbilityMaxSize)) {
        return true;
      }

      const resourcePool = groupBy(mainCards.flatMap(
        (c) => c.abilities.find((item) => item.type === config.mainAbility)?.values ?? []
      ), (item) => item);
      const otherCards = characters.filter((it) => !mainCards.includes(it));

      for (let i = 0; i < otherCards.length; i++) {
        const card = otherCards[i]

        const cardValues = card.abilities.find((item) => item.type === config.ability)?.values
        if (!cardValues || cardValues.length === 0) {
          return false
        }

        let valid = true;
        const otherCardResource = groupBy(cardValues, (item) => item);

        Object.entries(otherCardResource).forEach((item) => {
          const mainResourcePool = resourcePool[item[0]];
          if (!mainResourcePool){
            valid = false;
            return;
          }
          if (config.singleResourceMode){
            return;
          }
          if (mainResourcePool.length >= item[1].length){
            return;
          }

          valid = false;
        })

        if (!valid){
          return false;
        }
      }
      return true
    },

    checkChildOfRequirement(config: ChildOfRequirementConfig, characters: Character[]): boolean {
      const parent: Character = this.getCardById(config.parentId)
      if (!parent) {
        return false
      }

      return characters.every((c) => parent.allowedChildren.includes(c.id))
    },

    checkComputedRequirement(config: ComputedRequirementConfig, characters: CardAmount[]): boolean {
      const firstMatchingAbility = characters.filter((card) =>
        this.checkRequirement(config.firstAbilityRequirement, [], this.getCardById(card.id))
      )
      const secondMatchingAbility = characters.filter((card) =>
        this.checkRequirement(config.secondAbilityRequirement, [], this.getCardById(card.id))
      )

      if (firstMatchingAbility.length == 0 || secondMatchingAbility.length == 0) return true

      const firstAbilityComputed = firstMatchingAbility.flatMap((cardAmount) => {
        const card: Character = this.getCardById(cardAmount.id)
        const abilityValue = card.abilities.find((ab) => ab.type === config.firstAbilityValue)
        if (!abilityValue) return new Array(cardAmount.amount).fill(0)
        const parsedValue = Number.parseInt(abilityValue.values[0])
        if (isNaN(parsedValue)) return new Array(cardAmount.amount).fill(0)
        return new Array(cardAmount.amount).fill(parsedValue)
      })
      const secondAbilityComputed = secondMatchingAbility.flatMap((cardAmount) => {
        const card: Character = this.getCardById(cardAmount.id)
        const abilityValue = card.abilities.find((ab) => ab.type === config.secondAbilityValue)
        if (!abilityValue) return new Array(cardAmount.amount).fill(0)
        const parsedValue = Number.parseInt(abilityValue.values[0])
        if (isNaN(parsedValue)) return new Array(cardAmount.amount).fill(0)
        return new Array(cardAmount.amount).fill(parsedValue)
      })

      const firstValue = this.getComputedValue(firstAbilityComputed, config.firstAbilityCompute)
      const secondValue = this.getComputedValue(secondAbilityComputed, config.secondAbilityCompute)
      const result = this.compareValues(firstValue, secondValue, config.comparison)
      return result
    },

    getComputedValue(items: number[], type: ComputedType) {
      switch (type) {
        case ComputedType.Count:
          return items.length
        case ComputedType.Sum:
          return items.reduce((prev, cur) => prev + cur, 0)
      }
    },

    compareValues(firstValue: number, secondValue: number, type: ComputedComparisonType) {
      switch (type) {
        case ComputedComparisonType.Equal:
          return firstValue === secondValue
        case ComputedComparisonType.HigherThan:
          return firstValue >= secondValue
        case ComputedComparisonType.LowerThan:
          return firstValue <= secondValue
      }
    },

    registerFilter(element: HTMLInputElement) {
      let filter: FilterFilter = this.filters.find((filter: Filter) => filter.key === element.name)
      if (!filter) {
        filter = {
          key: element.name,
          filterItems: []
        }
        this.filters.push(filter)
      }

      filter.filterItems.push({
        value: element.value,
        option: element.checked ? ItemOption.Include : ItemOption.None,
        element: element
      })
    },

    updateFilter(event: InputEvent) {
      this.updateFilterByTarget(event.target)
    },

    updateFilterByTarget(target: HTMLInputElement) {
      const filterName: string = target.name
      const filterValue: string = target.value
      const filterCheck: boolean = target.checked

      this.updateFilterByValues(
        filterName,
        filterValue,
        filterCheck ? ItemOption.Include : ItemOption.None
      )
    },

    updateFilterByValues(name: string, value: string, option: ItemOption) {
      const filter: FilterFilter = this.filters.find((item: Filter) => item.key === name)
      if (!filter) {
        return
      }

      const filterItem = filter.filterItems.find((item) => item.value === value)
      if (!filterItem) {
        return
      }

      if (filterItem.option === option) {
        return
      }

      filterItem.option = option
      if (filterItem.option === ItemOption.Include) {
        filterItem.element.checked = true
      } else if (filterItem.option === ItemOption.None) {
        filterItem.element.checked = false
      }

      this.updateFilteredCards()
    },

    resetFilters() {
      this.filters.forEach((filter: FilterFilter) => {
        filter.filterItems.forEach((item) => {
          if (item.option === ItemOption.None) {
            return false
          }
          this.removeFilter(item)
        })
      })
    },

    removeFilter(filterItem: FilterItem) {
      filterItem.option = ItemOption.None
      if (filterItem.element) {
        filterItem.element.checked = false
      }
      this.updateFilteredCards()
    },

    handleSearchInput(event: KeyboardEvent) {
      event.preventDefault()
      this.updateFilteredCards()
    },

    updateFilteredCards() {
      const filters: { [key: string]: string[] } = {}
      this.filters.forEach((filter: FilterFilter) => {
        const selectedItems = filter.filterItems
          .filter((it) => it.option === ItemOption.Include)
          .map((it) => it.value)
        filters[filter.key] = selectedItems
      })

      this.loadingCards = true
      fetch('/umbraco/api/squadapi/searchcards', {
        method: 'POST',
        body: JSON.stringify({
          search: this.search,
          Filters: filters
        }),
        headers: {
          'Content-Type': 'application/json'
        }
      })
        .then((res) => res.json())
        .then((res) => {
          this.filteredCardIds = res
          this.loadingCards = false
        })
    },

    isVisible(card: Character): boolean {
      if (this.collectionMode) {
        return Object.keys(this.ownedCharacters).includes(card.id.toString())
      }

      if (!this.filteredCardIds) {
        return true
      }

      return this.filteredCardIds.includes(card.id)
    },

    hasEnoughInCollection(cardId: number, cardAmount: number): boolean {
      if (!this.collectionMode) {
        return true
      }

      if (!Object.keys(this.ownedCharacters).includes(cardId.toString())) {
        return false
      }

      return this.ownedCharacters[cardId.toString()] >= cardAmount
    },

    closeFilter(filterName: string) {
      if (this.openFilter === filterName) {
        this.openFilter = ''

        document.getElementsByTagName('body')[0].removeEventListener('click', this.onBodyClick)
      }
    },

    toggleFilter(filterName: string) {
      if (this.openFilter === filterName) {
        this.openFilter = ''
      } else {
        this.openFilter = filterName

        document.getElementsByTagName('body')[0].addEventListener('click', this.onBodyClick)
      }
    },

    onBodyClick(event: PointerEvent) {
      const tagName = (event.target as HTMLElement).closest('.js-filter')
      if (!tagName) {
        this.openFilter = ''
        document.getElementsByTagName('body')[0].removeEventListener('click', this.onBodyClick)
      }
    },
  }
})

app.mount('#builder')

function groupBy<T>(iterable: Iterable<T>, fn: (item: T) => string | number) {
  return [...iterable].reduce<Record<string, T[]>>((groups, curr) => {
      const key = fn(curr);
      const group = groups[key] ?? [];
      group.push(curr);
      return { ...groups, [key]: group };
  }, {});
}