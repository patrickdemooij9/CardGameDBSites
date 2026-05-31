/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { DeckJsonCard } from './DeckJsonCard';
import type { DeckJsonDeckCard } from './DeckJsonDeckCard';
import type { DeckJsonMetaData } from './DeckJsonMetaData';
export type DeckJsonFile = {
    metadata?: DeckJsonMetaData;
    leader?: (DeckJsonCard | DeckJsonDeckCard);
    base?: (DeckJsonCard | DeckJsonDeckCard);
    deck?: Array<DeckJsonDeckCard>;
};

