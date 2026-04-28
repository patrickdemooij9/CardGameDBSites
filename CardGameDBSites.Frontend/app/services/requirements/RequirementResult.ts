import type { RequirementApiModel } from "~/api/default";

export default interface RequirementResult {
    IsValid: boolean;
    ErrorRequirement?: RequirementApiModel;
}