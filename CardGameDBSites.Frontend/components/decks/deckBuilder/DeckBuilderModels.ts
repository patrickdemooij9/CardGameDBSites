import type { CardDetailApiModel } from "~/api/default";
import type CreateDeckSlot from "./models/CreateDeckSlot";
import type CreateDeckGroup from "./models/CreateDeckGroup";

export interface DeckCard {
    card: CardDetailApiModel;
    amount: number;
    allowRemoval: boolean;
    children: CreateDeckSlot[]
}

export interface CreateDeckPostModel {
    id: number;
    name: string;
    description: string;
}

export interface DeckAmount {
    type: string;
    config: { [key: string]: any };
}

export interface FixedSquadAmountConfig {
    amount: number;
}

export interface DynamicSquadAmountConfig {
    requirements: DeckRequirement[];
}

/*export interface RichCardAmount {
    card: Character;
    amount: number;
    allowRemoval: boolean;
    children: SquadSlot[];
}*/

export interface DeckMutation {
    alias: string;
    change: number;
    slotId: number;
}

export interface CardLocation {
    group: CreateDeckGroup | undefined;
    slot: CreateDeckSlot;
    isAllowed: boolean;
}

export interface CardLookup {
    group: CreateDeckGroup;
    slot: CreateDeckSlot;
}

export interface SlotRequirement {
    slotId: number;
    requirements: DeckRequirement[];
}

export interface DeckRequirement {
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

export enum ComputedType {
    Sum = "Sum",
    Count = "Count"
}

export enum ComputedComparisonType{
    Equal = "Equal",
    HigherThan = "HigherThan",
    LowerThan = "LowerThan"
}

export interface ComputedRequirementConfig {
    firstAbilityRequirement: { [key: string]: any },
    firstAbilityCompute: ComputedType,
    firstAbilityValue: string;
    comparison: ComputedComparisonType,
    secondAbilityRequirement: { [key: string]: any },
    secondAbilityCompute: ComputedType,
    secondAbilityValue: string;
}

export interface RequiredCardConfig {
    requiredCardId: number;
}

export interface ResourceRequirementConfig {
    mainAbility: string;
    mainAbilityMaxSize: number;
    ability: string;
    mainCardsCondition: { [key: string]: any }[];
    singleResourceMode: boolean;
}

export interface ChildOfRequirementConfig {
    parentId: number;
}

export enum RequirementType {
    UniqueValue = "UniqueValue",
    SameValue = "SameValue",
    NotEqualValue = "NotEqualValue",
    EqualValue = "EqualValue",
    Size = "Size",
    Conditional = "Conditional",
    RequiredCard = "RequiredCard",
    Resource = "Resource",
    ChildOf = "ChildOf",
    Computed = "Computed"
}

export enum DisplaySize {
    Small = "Small",
    Medium = "Medium"
}