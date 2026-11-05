import SetService from "~/services/SetService";

export function useAllSets() {
  return useAsyncData("allSets", () => new SetService().getAllSets(), { default: () => [] });
}
