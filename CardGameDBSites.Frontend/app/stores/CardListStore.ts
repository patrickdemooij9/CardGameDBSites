import type { CardListModel } from "~/models/CardListModel";

export const useCardListStore = defineStore("cardList", {
  state: () => ({
    lists: null as CardListModel[] | null,
  }),
  getters: {
    getLists: (state) => state.lists,
    getListById: (state) => {
      return (id: number) => state.lists?.find(l => l.id === id);
    }
  },
  actions: {
    setLists(lists: CardListModel[]) {
      this.lists = lists;
    },
    addList(list: CardListModel) {
      if (this.lists) {
        this.lists.push(list);
      } else {
        this.lists = [list];
      }
    },
    updateList(id: number, name: string, description: string | null | undefined, isPublic: boolean) {
      const list = this.lists?.find(l => l.id === id);
      if (list) {
        list.name = name;
        list.description = description;
        list.isPublic = isPublic;
      }
    },
    removeList(id: number) {
      if (this.lists) {
        this.lists = this.lists.filter(l => l.id !== id);
      }
    },
    clearLists() {
      this.lists = null;
    }
  },
});
