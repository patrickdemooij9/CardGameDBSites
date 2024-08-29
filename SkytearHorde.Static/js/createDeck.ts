export default(initModel: CreateDeckModel) => ({
    cards: initModel.allCards,
    selectedCards: initModel.initialCards.map<SelectedCard>(item => ({
        card: item,
        amount: item.startingAmount
    })),
    id: initModel.id,
    startingDeckId: initModel.startingDeckId,
    name: initModel.name,
    description: initModel.description,

    dataString: "",
    markdownPreview: false,
    markdownPreviewText: '',

    cardAmount: 41,

    deckName(){
        if (this.name === ""){
            return "Name of deck";
        }
        return this.name;
    },

    addCard(id: number){
        const card = this.cards.find((item) => item.id === id);
        if (card){
            const existedCard = this.selectedCards.find((item : SelectedCard) => item.card.id === id);
            if (existedCard){
                existedCard.amount++;
            }else{
                this.selectedCards.push({
                    card: card,
                    amount: 1
                });
            }
        }
    },

    removeCard(id: number){
        const cardToRemove = this.selectedCards.find((item) => item.card.id === id);
        if (cardToRemove){

            if (cardToRemove.amount == 1){
                const index = this.selectedCards.indexOf(cardToRemove);
                this.selectedCards.splice(index, 1);
            }else{
                cardToRemove.amount--;
            }
            
        }
    },

    getSelectedCards(group: string) : Card[]{
        return this.selectedCards.filter((item : SelectedCard) => item.card.group.toLocaleLowerCase() === group.toLocaleLowerCase());
    },

    isEmpty(group: string){
        return this.getSelectedCards(group).length === 0;
    },

    maxReached(card: Card): boolean{
        var selectedCard = this.selectedCards.find((item : SelectedCard) => item.card.id === card.id);
        if (!selectedCard) { return false; }
        return selectedCard.amount >= card.maxAmount;
    },

    cardCountOfRarity(rarity: Rarity): number{
        var items = this.selectedCards.filter((item: SelectedCard) => item.card.rarity === rarity).map((item) => item.amount);
        if (items.length === 0) {return 0;}
        return items.reduce((partialSum, item) => partialSum + item);
    },

    currentDeckAmount(excludeCastle: boolean): number{
        return this.selectedCards.map((item) => item.amount).reduce((partialSum, item) => partialSum + item) - (excludeCastle ? 1 : 0);
    },

    validDeckAmount(): boolean{
        const currentAmount = this.currentDeckAmount(false);
        return currentAmount === this.cardAmount;
    },

    validLegendaryAmount(): boolean{
        return this.cardCountOfRarity(Rarity.Legendary) <= 2;
    },

    validMythicAmount(): boolean{
        return this.cardCountOfRarity(Rarity.Mythic) <= 6;
    },

    isValidForSave(): boolean{
        return this.validLegendaryAmount() && this.validMythicAmount();
    },

    isValidDeck(): boolean{
        return this.validDeckAmount() && this.validLegendaryAmount() && this.validMythicAmount();
    },

    submitForm(){
        this.dataString = JSON.stringify({
            id: this.id,
            startingDeckId: this.startingDeckId,
            name: this.name,
            description: this.description,
            cards: this.selectedCards.map(item => ({
                cardId: item.card.id,
                amount: item.amount
            }))
        });
    },

    getMainCard(): Card{
        return this.selectedCards.find((item) => item.card.group === "Castle").card;
    },

    async toggleMarkdownPreview(){

        if (!this.markdownPreview){
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
    }
});

enum Rarity{
    Legendary = "Legendary",
    Mythic = "Mythic",
    Rare = "Rare",
    Base = "Base",
    None = "None"
}

interface AbilityGroup{
    name: string;
    abilities: string[];
}

interface Card{
    id: number,
    name: string,
    group: string,
    maxAmount: number,
    startingAmount: number,
    imageUrl: string,
    thumbnailUrl: string,
    rarity: Rarity,
    overviewImage: string,
    abilityGroups: AbilityGroup[]
}

interface SelectedCard{
    card: Card;
    amount: number;
}

interface CreateDeckModel{
    id: number,
    startingDeckId: string,
    name: string,
    description: string,
    initialCards: Card[],
    allCards: Card[]
}

enum Types{
    ally = "ally",
    tower = "tower",
    attachment = "attachment",
    castle = "castle"
}