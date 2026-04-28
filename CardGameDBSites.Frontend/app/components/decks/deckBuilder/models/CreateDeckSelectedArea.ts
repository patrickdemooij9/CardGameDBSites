import type CreateDeckGroup from "./CreateDeckGroup";
import type CreateDeckSlot from "./CreateDeckSlot";

export interface CreateDeckSelectedArea {
    slot: CreateDeckSlot;
    group: CreateDeckGroup;
}