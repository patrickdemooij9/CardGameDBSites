export default (config: Config) => ({

    filters: [],

    init() {
        window.addEventListener("SetFilter", (event: CustomEvent) => {
            this.updateFilterByValues(event.detail.name, event.detail.value, event.detail.option);
        });
        window.addEventListener("ResetFilters", () => this.resetFilters());

    },

    registerFilter(element: HTMLInputElement) {
        var filter: Filter = this.filters.find((filter: Filter) => filter.key === element.name);
        if (!filter) {
            filter = {
                key: element.name,
                filterItems: []
            };
            this.filters.push(filter);
        }

        filter.filterItems.push({
            value: element.value,
            option: element.checked ? ItemOption.Include : ItemOption.None,
            element: element
        });
    },

    updateFilter(event) {
        this.updateFilterByTarget(event.target);
    },

    updateFilterByTarget(target) {
        const filterName: string = target.name;
        const filterValue: string = target.value;
        const filterCheck: boolean = target.checked;

        this.updateFilterByValues(filterName, filterValue, filterCheck ? ItemOption.Include : ItemOption.None);
    },

    updateFilterByValues(name: string, value: string, option: ItemOption) {
        const filter: Filter = this.filters.find((item: Filter) => item.key === name);
        if (!filter) { return; }

        const filterItem = filter.filterItems.find(item => item.value === value);
        if (!filterItem) { return; }

        if (filterItem.option === option) { return; }

        filterItem.option = option;
        if (filterItem.element) {
            if (filterItem.option === ItemOption.Include) {
                filterItem.element.checked = true;
            } else if (filterItem.option === ItemOption.None) {
                filterItem.element.checked = false;
            }
        }

        this.updateUrl();
    },

    addFilterValue(name: string, value: string)
    {
        var filter: Filter = this.filters.find((filter: Filter) => filter.key === name);
        if (!filter) {
            filter = {
                key: name,
                filterItems: []
            };
            this.filters.push(filter);
        }
        filter.filterItems.push({
            value: value,
            option: ItemOption.Include
        });
    },

    resetFilters() {
        this.filters.forEach((filter: Filter) => {
            filter.filterItems.forEach(item => {
                if (item.option === ItemOption.None) { return false; }
                this.removeFilter(item);
            })
        })
    },

    removeFilter(filterItem: FilterItem) {
        filterItem.option = ItemOption.None;
        if (filterItem.element) {
            filterItem.element.checked = false;
        }

        this.updateUrl();
    },

    updateUrl() {
        if (!config.changeUrl) {
            return;
        }

        var url = new URL(window.location.href.split('?')[0]);

        this.filters.forEach((filter: Filter) => {
            const checkedItems = filter.filterItems.filter(item => item.option === ItemOption.Include);
            if (checkedItems.length === 0) {
                url.searchParams.delete(filter.key);
                return;
            }
            checkedItems.forEach(item => {
                url.searchParams.append(filter.key, item.value);
            });
        });

        history.replaceState({}, null, url);
    }
});

interface Config {
    changeUrl: boolean;
}

interface Filter {
    key: string,
    filterItems: FilterItem[]
}

interface FilterItem {
    value: string,
    option: ItemOption,
    element?: HTMLInputElement
}

interface CardValue {
    type: string,
    value: string,
    values: string[]
}

enum ItemOption {
    None,
    Include,
    Exclude
}