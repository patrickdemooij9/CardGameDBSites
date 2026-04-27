/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { ApiBlockListModel } from './ApiBlockListModel';
import type { IApiMediaWithCropsModel } from './IApiMediaWithCropsModel';
export type SquadSettingsPropertiesModel = {
    typeID?: number | null;
    typeDisplayName?: string | null;
    squads?: ApiBlockListModel;
    restrictions?: ApiBlockListModel;
    defaultNames?: Array<string> | null;
    specialImageRule?: ApiBlockListModel;
    slotOnlyMode?: boolean | null;
    overwriteAmount?: number | null;
    maxDynamicSlots?: number | null;
    mainCard?: ApiBlockListModel;
    useDeckDisplay?: boolean | null;
    useCompactDeckDisplay?: boolean | null;
    showDeckColors?: boolean | null;
    amountOfSquadCards?: number | null;
    colorLogic?: ApiBlockListModel;
    costIcon?: Array<IApiMediaWithCropsModel> | null;
    renderCostInIcon?: boolean | null;
};

