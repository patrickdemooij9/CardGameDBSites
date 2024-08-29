export interface CreateSquadModel {
    id: number;
    name: string;
    description: string,
    isLoggedIn: boolean,
    squads: Squad[];
    allCharacters: Character[];
    requirements: SquadRequirement[];
    preselectFirstSlot: boolean;

    hasDynamicSlot: boolean; // True if there are slots with ShowIfTargetSlotIsFilled
}

export interface Squad {
    id: number;
    name: string;
    slots: SquadSlot[];
    requirements: SquadRequirement[];
}

export interface CharacterCurrentSpot {
    squad: Squad,
    slot: SquadSlot,
}

export interface SquadSlot {
    id: number,
    //cardIds: CardAmount[];
    label: string;
    requirements: SquadRequirement[];
    cardGroups: SquadGrouping[];

    minCards: number;
    maxCards: number;
    displaySize: DisplaySize;
    disableRemoval: boolean;
    numberMode: boolean;
    showIfTargetSlotIsFilled: number | undefined;

    additionalFilterRequirements: SquadRequirement[];
}

export interface SquadGrouping {
    displayName: string;
    sortBy: string;
    cardIds: CardAmount[];

    requirements: SquadRequirement[];
}

export interface CardAmount {
    id: number;
    amount: number;
    allowRemoval: boolean;
}

export interface Character {
    id: number;
    name: string;
    abilities: Ability[];
    iconUrls: string[];
    images: CharacterImage[];
    teamRequirements: SquadRequirement[];
    squadRequirements: SquadRequirement[];
    slotRequirements: SlotRequirement[];

    // Parameters for easier data access
    validLocations: CardLocation[];
    presentInSlot: CharacterCurrentSpot | undefined;
    needsReindexing: boolean | undefined;
}

export interface CharacterImage {
    imageUrl: string;
    label: string;
    isPrimaryImage: boolean;
}

export interface CardLocation {
    squad: Squad;
    slot: SquadSlot;
    isAllowed: boolean;
}

export interface CardLookup {
    squad: Squad;
    slot: SquadSlot;
}

export interface SlotRequirement {
    slotId: number;
    requirements: SquadRequirement[];
}

export interface SquadRequirement {
    type: string;
    config: { [key: string]: any };
}

export interface UniqueValueRequirementConfig {
    ability: string;
}

export interface SameValueRequirementConfig {
    ability: string;
}

export interface NotEqualValueRequirementConfig {
    ability: string;
    values: string[];
}

export interface EqualValueRequirementConfig {
    ability: string;
    values: string[];
}

export interface SizeRequirementConfig {
    min: number | undefined;
    max: number | undefined;
}

export interface ConditionalRequirementConfig {
    condition: { [key: string]: any }[];
    requirements: { [key: string]: any }[];
}

export interface RequiredCardConfig {
    requiredCardId: number;
}

export interface ResourceRequirementConfig {
    ability: string;
    mainCardsCondition: { [key: string]: any }[];
    requireAllResources: boolean;
}

export interface Ability {
    displayName: string;
    type: string;
    values: string[];
    displayValue: string;
}

export interface Filter {
    key: string,
    value: string,
    excluded: boolean
}

export enum Tab {
    Squad = 'squad',
    Cards = 'cards',
    Details = 'details'
}

export enum RequirementType {
    UniqueValue = "UniqueValue",
    SameValue = "SameValue",
    NotEqualValue = "NotEqualValue",
    EqualValue = "EqualValue",
    Size = "Size",
    Conditional = "Conditional",
    RequiredCard = "RequiredCard",
    Resource = "Resource"
}

export enum DisplaySize {
    Small = "Small",
    Medium = "Medium"
}