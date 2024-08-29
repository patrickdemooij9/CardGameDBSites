export default (initModel: CreateSquadModel) => ({
    id: initModel.id,
    name: initModel.name,
    description: initModel.description,
    isLoggedIn: initModel.isLoggedIn,
    requirements: initModel.requirements,

    characters: initModel.allCharacters,
    squads: initModel.squads,

    selectedCharacter: undefined,
    selectedCharacterImage: undefined,
    selectedSquad: undefined,
    selectedSlot: undefined,

    dataString: "",
    currentTab: Tab.Squad,

    markdownPreview: false,
    markdownPreviewText: '',

    init() {
        this.squads.forEach((squad: Squad) => {
            squad.slots.forEach(slot => {
                if (!slot.cardIds) { slot.cardIds = []; }

                slot.cardIds.forEach(cardAmount => {
                    const card: Character = this.getCardById(cardAmount.id);
                    card.presentInSlot = { squad, slot };

                    this.addRequirementsByCharacter(card, squad);
                });
            });
        });

        this.indexCharacters();

        if (initModel.preselectFirstSlot) {
            let foundSlot = false;
            this.squads.forEach((squad: Squad) => {
                const nonFilledSlot = squad.slots.find(s => !this.isSlotFilled(s));
                if (nonFilledSlot !== undefined) {
                    this.clickSlot(squad, nonFilledSlot);
                    foundSlot = true;
                    return;
                }
            });

            if (!foundSlot) {
                const allSlots: SquadSlot[] = this.squads.flatMap((s: Squad) => s.slots);
                let slotToClick = allSlots.find(s => !this.isSlotFilled(s));
                if (!slotToClick) {
                    slotToClick = allSlots[allSlots.length - 1];
                }
                const lastSquad = this.squads[this.squads.length - 1];
                this.clickSlot(lastSquad, lastSquad.slots[lastSquad.slots.length - 1]);
            }
        }
    },

    indexCharacters() {
        this.characters.forEach((character: Character) => {
            this.indexCharacter(character);
        });
        console.log(this.characters);
    },

    indexCharacter(character: Character) {
        character.validLocations = [];

        if (character.presentInSlot) {
            character.validLocations.push({ squad: character.presentInSlot.squad, slot: character.presentInSlot.slot, isAllowed: !this.isSlotFilled(character.presentInSlot.slot) });
            return;
        }

        if (!this.isAllowedForTeam(character)) { return; }

        this.squads.forEach((squad: Squad) => {
            if (!this.isAllowedForSquad(squad, character)) { return; }

            squad.slots.forEach(slot => {
                const isSlotFilled = this.isSlotFilled(slot);
                const isInSlot = slot.cardIds.some(card => card.id === character.id);

                if (!isInSlot) {
                    if (!this.isAllowedForSlot(slot, character)) { return; }
                }

                character.validLocations.push({ squad: squad, slot: slot, isAllowed: !isSlotFilled || isInSlot });
            });
        })
    },

    addToSquad(squad: Squad, slot: SquadSlot, character: Character) {
        this.closeModal();

        const cardAmount = slot.cardIds.find(card => card.id === character.id);

        if (cardAmount) {
            cardAmount.amount++;
        } else {
            slot.cardIds.push({
                id: character.id,
                amount: 1
            });
            character.presentInSlot = { squad, slot: slot };

            if (!initModel.preselectFirstSlot && this.isSlotFilled(slot)) {
                this.selectedSquad = undefined;
                this.selectedSlot = undefined;
            }

            this.addRequirementsByCharacter(character, squad);
            this.indexCharacters();
        }

        /*if (this.isSlotFilled(characterSlot)) {
            var currentSlotIndex = squad.slots.indexOf(characterSlot);
            if (currentSlotIndex === squad.slots.length - 1) { return; }

            this.clickSlot(squad, squad.slots[currentSlotIndex + 1]);
        }*/
    },

    removeCharacterFromSquad(character: Character) {
        const slot = character.presentInSlot?.slot;
        if (!slot) { return; }

        var wasFilled = this.isSlotFilled(slot);

        const cardAmount = slot.cardIds.find(card => card.id === character.id);
        if (cardAmount.amount > 1) {
            cardAmount.amount--;
            if (wasFilled) {
                this.indexCharacters();
            }

            return;
        }

        this.closeModal();

        const lookupResult: CardLookup = this.getSlotByCharacter(character);
        character.teamRequirements.forEach(tr => {
            const index = this.requirements.indexOf(tr, 0);
            if (index > -1) {
                this.requirements.splice(index, 1);
            }
        });
        character.squadRequirements.forEach(sr => {
            const index = lookupResult.squad.requirements.indexOf(sr, 0);
            if (index > -1) {
                lookupResult.squad.requirements.splice(index, 1);
            }
        });
        character.slotRequirements.forEach(sr => {
            const slot = lookupResult.squad.slots[sr.slotId];
            sr.requirements.forEach(r => {
                const index = slot.requirements.indexOf(r, 0);
                if (index > -1) {
                    slot.requirements.splice(index, 1);
                }
            })
        })

        slot.cardIds.splice(slot.cardIds.indexOf(cardAmount), 1);
        character.presentInSlot = undefined;

        console.log(this.squads);

        this.indexCharacters();
    },

    removeFromSquad(characterSlot: SquadSlot) {
        this.closeModal();
        characterSlot.cardIds.forEach(cardId => {
            this.removeCharacterFromSquad(this.getCardById(cardId.id));
        });
    },

    addRequirementsByCharacter(character: Character, squad: Squad) {
        character.teamRequirements.forEach(tr => {
            this.requirements.push(tr);
        });
        character.squadRequirements.forEach(sr => {
            squad.requirements.push(sr);
        });
        character.slotRequirements.forEach(sr => {
            squad.slots[sr.slotId].requirements.push(...sr.requirements);
        });
    },

    clickSlot(squad: Squad, slot: SquadSlot) {
        const element = document.getElementById('allCharacters');

        if (element) {
            element.scrollIntoView();
        }

        window.dispatchEvent(new CustomEvent("ResetFilters"));

        /*const requirements : SquadRequirement[] = [];
        requirements.push(...this.teamRequirements);
        requirements.push(...squad.requirements);
        requirements.push(...slot.requirements);

        requirements.forEach(requirement => {
            const filters : Filter[] = this.requirementToFilter(requirement);
            if (!filters) { return; }

            filters.forEach(filter => {
                window.dispatchEvent(new CustomEvent("SetFilter", {
                    detail: {
                        name: filter.key,
                        value: filter.value,
                        option: filter.excluded ? 2 : 1
                    }
                }));
            });
        });*/

        this.selectedSquad = squad;
        this.selectedSlot = slot;
    },

    selectCharacter(character: Character) {
        this.selectedCharacter = character;
        this.selectedCharacterImage = character.images.find((item) => item.isPrimaryImage);

        document.querySelector("body").classList.add('modal-open');
    },

    selectImage(image: CharacterImage) {
        this.selectedCharacterImage = image;
    },

    closeModal() {
        this.selectedCharacter = undefined;
        this.selectedCharacterImage = undefined;

        document.querySelector("body").classList.remove('modal-open');
    },

    clickTab(tab: Tab) {
        this.currentTab = tab;
    },

    async toggleMarkdownPreview() {

        if (!this.markdownPreview) {
            const response = await fetch("/umbraco/api/markdown/preview", {
                method: 'POST',
                body: JSON.stringify({
                    markdown: this.description
                }),
                headers: {
                    'Content-type': 'application/json; charset=UTF-8',
                }
            });
            const body = await response.text();
            this.markdownPreviewText = body;
        }

        this.markdownPreview = !this.markdownPreview;
    },

    getCharacterSpotValidForSquad(squad: Squad, character: Character): SquadSlot | undefined {
        return squad.slots.find(slot => {
            if (!this.isSlotFilled(slot) && !this.isSlotFillForCard(slot, character) && character.validLocations.some(l => l.slot === slot && l.squad === squad)) {
                return true;
            }
        });
    },

    getValidLocations(character: Character){
        return character.validLocations;
    },

    isLocationAllowed(location: CardLocation) : boolean{
        return location.isAllowed;
    },

    isPresentInSlot(location: CardLocation, character: Character) : boolean{
        if (!character.presentInSlot) { return false; }
        return location.slot.cardIds.some(it => it.id === character.id); 
    },

    isAllowedForAll(squad: Squad, slot: SquadSlot, character: Character): boolean {
        if (!this.isAllowedForTeam(character)) { return false; }
        if (!this.isAllowedForSquad(squad, character)) { return false; }
        return this.isAllowedForSlot(slot, character, []);
    },

    isAllowedForTeam(character: Character): boolean {
        let valid = true;

        const allCharactersInSquads = this.squads.flatMap((s: Squad) => s.slots).flatMap((s: SquadSlot) => s.cardIds).map(it => this.getCardById(it.id));
        //Check the team requirements
        this.requirements.forEach((requirement: SquadRequirement) => {
            if (!this.checkRequirement(requirement, allCharactersInSquads, character)) {
                valid = false;
            }
        });

        if (!valid) {
            return false;
        }

        //Check specific character team requirements
        if (character.teamRequirements.length > 0) {
            character.teamRequirements.forEach(requirement => {
                if (!this.checkRequirement(requirement, allCharactersInSquads, character)) {
                    valid = false;
                }
            });
        }
        return valid;
    },

    isAllowedForSquad(squad: Squad, character: Character): boolean {
        let valid = true;

        //Check the squad requirements
        const squadCharacters = squad.slots.flatMap(it => it.cardIds).map(it => this.getCardById(it.id));
        squad.requirements.forEach(requirement => {
            if (!this.checkRequirement(requirement, squadCharacters, character)) {
                valid = false;
            }
        })

        if (!valid) {
            return false;
        }

        //Check specific character squad requirements
        if (character.squadRequirements.length > 0) {
            character.squadRequirements.forEach(requirement => {
                if (!this.checkRequirement(requirement, squadCharacters, character)) {
                    valid = false;
                }
            });
        }
        return valid;
    },

    isAllowedForSlot(slot: SquadSlot, character: Character): boolean {
        let valid = true;

        //Check the slot requirement
        slot.requirements.forEach(requirement => {
            if (!this.checkRequirement(requirement, [], character)) {
                valid = false;
            }
        });

        if (!valid) { return false; }

        if (character.slotRequirements.length > 0) {
            if (this.characters.some((c: Character) => !this.checkTargetRequirements(character, c))) {
                valid = false;
            }
        }

        return valid;
    },

    checkTargetRequirements(character: Character, characterToCheck: Character) {
        if (!characterToCheck.presentInSlot) { return true; }
        const requirements = character.slotRequirements.filter(r => r.slotId === characterToCheck.presentInSlot.slot.id).flatMap(r => r.requirements);
        if (requirements.length === 0) { return true; }

        return requirements.every(r => this.checkRequirement(r, [], characterToCheck));
    },

    isSlotFilled(slot: SquadSlot): boolean {
        return this.getSlotAmount(slot) === slot.maxCards;
    },

    isSlotFillForCard(slot: SquadSlot, character: Character): boolean {
        const cardAmount = slot.cardIds.find(it => it.id === character.id);
        if (!cardAmount) return false;
        return cardAmount.amount === this.getMaxSizeForSlot(character);
    },

    getSquadAmount(squad: Squad): number {
        return squad.slots.reduce((sum, slot) => sum + this.getSlotAmount(slot), 0);
    },

    getMaxSquadAmount(squad: Squad): number {
        return squad.slots.reduce((sum, slot) => sum + slot.maxCards, 0);
    },

    getSlotAmount(slot: SquadSlot): number {
        return slot.cardIds.reduce((sum, value) => sum + value.amount, 0)
    },

    getFilteredCharacters(): Character[] {
        if (!this.selectedSquad) return [];

        return this.characters.filter((character: Character) => {
            if (character.validLocations.some((l) => l.slot === this.selectedSlot && l.squad === this.selectedSquad)) {
                return true;
            }
            return false;
        })
    },

    isShownInOverview(character: Character): boolean {
        if (!this.selectedSquad || !this.selectedSlot) return true;

        return character.validLocations.find(l => l.squad === this.selectedSquad && l.slot === this.selectedSlot) !== undefined;
    },

    getSlotByCharacter(character: Character): CardLookup {
        let lookup: CardLookup | undefined = undefined;

        this.squads.forEach((squad: Squad) => {
            var foundSlot = squad.slots.find((slot) => slot.cardIds.some(it => it.id === character.id));
            if (foundSlot) {
                lookup = new CardLookup();
                lookup.slot = foundSlot;
                lookup.squad = squad;
            }
        });
        return lookup;
    },

    getMaxSizeForSlot(character: Character): number {
        var values = this.getAbilityValueByType(character, "Amount");
        if (!values) { return 1; }
        return parseInt(values[0]);
    },

    isPresentInSquad(character: Character): boolean {
        return this.getSlotByCharacter(character);
    },

    getCardById(id: number | string): Character | undefined {
        if (typeof id === "string") {
            id = parseInt(id);
        }

        return this.characters.find(item => item.id === id);
    },

    getAbilityValueByType(character: Character, type: string): string[] {
        return character.abilities.find((item) => item.type === type)?.values;
    },

    getAbilityDisplayByType(character: Character, type: string): string {
        return character?.abilities.find((item) => item.type === type)?.displayValue;
    },

    hasAbility(character: Character, type: string) {
        const ability = character.abilities.find((item) => item.type === type);
        return !!ability;
    },

    hasAbilityValue(character: Character, type: string, value: string) {
        const ability = character.abilities.find((item) => item.type === type);
        if (!ability) { return false; }

        return ability.values.includes(value);
    },

    hasMainImage(character: Character) {
        return character.images.some((item) => item.isPrimaryImage)
    },

    getMainImageUrl(character: Character) {
        return "https://shatterpointdb.com" + character.images.find((item) => item.isPrimaryImage).imageUrl;
    },

    pointsLeft(squad: Squad) {
        const charactersInSquad = squad.slots.flatMap(it => it.cardIds).map(it => this.getCardById(it.id));
        var points = 0;

        charactersInSquad.forEach((character) => {
            const pointsToAdd: string[] = this.getAbilityValueByType(character, 'Points');
            if (pointsToAdd) {
                points += parseInt(pointsToAdd[0]);
            }
        });
        return points;
    },

    checkRequirement(requirementConfig: SquadRequirement, characters: Character[], character: Character): boolean {
        const checkCharacters = [...characters];
        if (character) {
            checkCharacters.push(character);
        }
        if (requirementConfig.type === RequirementType.UniqueValue) {
            return this.checkUniqueValueRequirement(requirementConfig.config as SameValueRequirementConfig, checkCharacters);
        }
        else if (requirementConfig.type === RequirementType.SameValue) {
            return this.checkSameValueRequirement(requirementConfig.config as UniqueValueRequirementConfig, checkCharacters);
        }
        else if (requirementConfig.type === RequirementType.NotEqualValue) {
            return this.checkNotEqualValueRequirement(requirementConfig.config as NotEqualValueRequirementConfig, checkCharacters);
        }
        else if (requirementConfig.type === RequirementType.EqualValue) {
            return this.checkEqualValueRequirement(requirementConfig.config as EqualValueRequirementConfig, checkCharacters);
        } else if (requirementConfig.type === RequirementType.Size) {
            return this.checkSizeRequirement(requirementConfig.config as SizeRequirementConfig, checkCharacters);
        }
        else if (requirementConfig.type === RequirementType.Conditional) {
            return this.checkConditionalRequirement(requirementConfig.config as ConditionalRequirementConfig, checkCharacters);
        }
        console.error("Requirement not found!");
        return false;
    },

    requirementToFilter(requirementConfig: SquadRequirement): Filter[] | undefined {
        if (requirementConfig.type === RequirementType.EqualValue) {
            const filters: Filter[] = [];
            const equalValueConfig = requirementConfig.config as EqualValueRequirementConfig;
            equalValueConfig.values.forEach(v => {
                filters.push({
                    key: equalValueConfig.ability,
                    value: v,
                    excluded: false
                });
            })
            return filters;
        }
        if (requirementConfig.type === RequirementType.NotEqualValue) {
            const filters: Filter[] = [];
            const equalValueConfig = requirementConfig.config as NotEqualValueRequirementConfig;
            equalValueConfig.values.forEach(v => {
                filters.push({
                    key: equalValueConfig.ability,
                    value: v,
                    excluded: true
                });
            })
            return filters;
        }
        return undefined;
    },

    hasEnoughPoints(squad: Squad): Boolean {
        return this.pointsLeft(squad) >= 0;
    },

    isValidForSave(): boolean {
        return true;
    },

    isValidDeck(): boolean {
        var valid = true;
        this.squads.forEach((squad: Squad) => {
            if (squad.slots.find(slot => !this.isSlotFilled(slot))) { valid = false; return; }
            if (!this.hasEnoughPoints(squad)) { valid = false; return; }
        });
        return valid;
    },

    submitForm() {
        this.dataString = JSON.stringify({
            id: this.id,
            name: this.name,
            description: this.description,
            squads: this.squads.map((item: Squad) => ({
                id: item.id,
                slots: item.slots.map(slot => ({
                    id: slot.id,
                    cards: slot.cardIds.map(key => ({
                        cardId: key.id,
                        amount: key.amount
                    }))
                }))
            }))
        });
    },

    checkUniqueValueRequirement(config: UniqueValueRequirementConfig, characters: Character[]): boolean {
        let buffer: string[] = [];

        for (let i = 0; i < characters.length; i++) {
            const character = characters[i];

            const values = character.abilities.find((item) => item.type === config.ability)?.values;
            if (values) {
                if (buffer.some(item => values.includes(item))) { return false; }

                values.forEach(value => {
                    buffer.push(value);
                });
            }
        }
        return true;
    },

    checkSameValueRequirement(config: SameValueRequirementConfig, characters: Character[]): boolean {
        let buffer: string[];

        for (let i = 0; i < characters.length; i++) {
            const character = characters[i];

            const values = character.abilities.find((item) => item.type === config.ability)?.values;
            if (!values) {
                return false;
            }

            if (!buffer) { buffer = values; }
            if (values.every(item => !buffer.includes(item))) { return false; }
        }
        return true;
    },

    checkNotEqualValueRequirement(config: NotEqualValueRequirementConfig, characters: Character[]): boolean {
        return characters.find((character) => {
            const characterValues = character.abilities.find((item) => item.type === config.ability)?.values;
            if (characterValues && characterValues.some(v => config.values.includes(v))) {
                return true;
            }
            return false;
        }) === undefined;
    },

    checkEqualValueRequirement(config: EqualValueRequirementConfig, characters: Character[]): boolean {
        return characters.find((character) => {
            const characterValues = character.abilities.find((item) => item.type === config.ability)?.values;
            if (!characterValues || !characterValues.some(v => config.values.includes(v))) {
                return true;
            }
            return false;
        }) === undefined;
    },

    checkSizeRequirement(config: SizeRequirementConfig, characters: Character[]): boolean {
        const size = characters.length;
        if (config.min && size < config.min) { return false; }
        if (config.max && size > config.max) { return false; }
        return true;
    },

    checkConditionalRequirement(config: ConditionalRequirementConfig, characters: Character[]): boolean {
        const conditionResult = config.condition.every((config) => this.checkRequirement(config, characters));
        if (conditionResult) {
            return config.requirements.every((config) => this.checkRequirement(config, characters));
        }
        return true;
    }
});

interface CreateSquadModel {
    id: number;
    name: string;
    description: string,
    isLoggedIn: boolean,
    squads: Squad[];
    allCharacters: Character[];
    requirements: SquadRequirement[];
    preselectFirstSlot: boolean;
}

interface Squad {
    id: number;
    name: string;
    slots: SquadSlot[];
    requirements: SquadRequirement[];
}

interface CharacterCurrentSpot {
    squad: Squad,
    slot: SquadSlot,
}

interface SquadSlot {
    id: number,
    cardIds: CardAmount[];
    label: string;
    requirements: SquadRequirement[];
    maxCards: number;
    displaySize: DisplaySize;
}

interface CardAmount {
    id: number;
    amount: number;
}

interface Character {
    id: number;
    name: string;
    abilities: Ability[];
    iconUrl: string;
    images: CharacterImage[];
    teamRequirements: SquadRequirement[];
    squadRequirements: SquadRequirement[];
    slotRequirements: SlotRequirement[];

    // Parameters for easier data access
    validLocations: CardLocation[];
    presentInSlot: CharacterCurrentSpot;
}

interface CharacterImage {
    imageUrl: string;
    label: string;
    isPrimaryImage: boolean;
}

interface CardLocation {
    squad: Squad;
    slot: SquadSlot;
    isAllowed: boolean;
}

class CardLookup {
    squad: Squad;
    slot: SquadSlot;
}

interface SlotRequirement {
    slotId: number;
    requirements: SquadRequirement[];
}

interface SquadRequirement {
    type: string;
    config: { [key: string]: any };
}

interface UniqueValueRequirementConfig {
    ability: string;
}

interface SameValueRequirementConfig {
    ability: string;
}

interface NotEqualValueRequirementConfig {
    ability: string;
    values: string[];
}

interface EqualValueRequirementConfig {
    ability: string;
    values: string[];
}

interface SizeRequirementConfig {
    min: number | undefined;
    max: number | undefined;
}

interface ConditionalRequirementConfig {
    condition: { [key: string]: any }[];
    requirements: { [key: string]: any }[];
}

interface Ability {
    displayName: string;
    type: string;
    values: string[];
    displayValue: string;
}

interface Filter {
    key: string,
    value: string,
    excluded: boolean
}

enum Tab {
    Squad = 'squad',
    Details = 'details'
}

enum RequirementType {
    UniqueValue = "UniqueValue",
    SameValue = "SameValue",
    NotEqualValue = "NotEqualValue",
    EqualValue = "EqualValue",
    Size = "Size",
    Conditional = "Conditional"
}

enum DisplaySize {
    Small = "Small",
    Medium = "Medium"
}