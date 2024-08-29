export default (config: Config) => ({

    filters: [],
    loading: false,
    pageNumber: undefined,

    init() {
        window.addEventListener("SetFilter", (event: CustomEvent) => {
            this.updateFilterByValues(event.detail.name, event.detail.value, event.detail.option);
        });
        window.addEventListener("ResetFilters", () => this.resetFilters());

        this.$refs.submitForm.addEventListener("submit", (event: SubmitEvent) => {
            event.preventDefault();
            this.updateOverview();
        });
    },

    registerFilter(element: HTMLInputElement, name: string) {
        var filter: Filter = this.filters.find((filter: Filter) => filter.key === name);
        if (!filter) {
            filter = {
                key: name,
                filterItems: [],
                elementLess: false
            };
            this.filters.push(filter);
        }

        filter.filterItems.push({
            value: element.value,
            label: element.value,
            option: element.checked ? ItemOption.Include : ItemOption.None,
            element: element
        });
    },

    updateFilter(event, name: string) {
        this.updateFilterByTarget(event.target, name);
    },

    updateFilterByTarget(target, name: string) {
        const filterValue: string = target.value;
        const filterCheck: boolean = target.checked;

        this.updateFilterByValues(name, filterValue, filterCheck ? ItemOption.Include : ItemOption.None);
    },

    updateFilterByValues(name: string, value: string, option: ItemOption) {
        const filter: Filter = this.filters.find((item: Filter) => item.key === name);
        if (!filter) { return; }

        const filterItem = filter.filterItems.find(item => item.value === value);
        if (!filterItem) { return; }

        if (filterItem.option === option) { return; }

        filterItem.option = option;
        if (filterItem.element){
            if (filterItem.option === ItemOption.Include) {
                filterItem.element.checked = true;
            } else if (filterItem.option === ItemOption.None) {
                filterItem.element.checked = false;
            }
        }

        this.updateUrl();
        this.updateOverview();
    },

    addFilterValue(name: string, value: string, label: string)
    {
        var filter: Filter = this.filters.find((filter: Filter) => filter.key === name);
        if (!filter) {
            filter = {
                key: name,
                filterItems: [],
                elementLess: true
            };
            this.filters.push(filter);
        }
        filter.filterItems.push({
            value: value,
            label: label,
            option: ItemOption.Include
        });

        this.updateUrl();
        this.updateOverview();
    },

    setPageNumber(page: number){
        this.pageNumber = page;

        this.updateUrl();
        this.updateOverview();
    },

    updateOverview(){
        const target = this.$refs.submitForm;
        var data = new FormData(target);
        data.append("isAjax", "true");
        if (this.pageNumber){
            data.append("pageNumber", this.pageNumber);
        }

        const elementLessFilters = this.filters.filter((item: Filter) => item.elementLess) as Filter[];
        elementLessFilters.forEach(filter => {
            const items = filter.filterItems.filter((item) => item.option === ItemOption.Include);
            if (items.length === 0) return;

            data.append(`Filters[${filter.key}]`, items.map((item) => item.value).join(","));
        });

        this.loading = true;
        fetch(this.$refs.submitForm.action, {
            method: 'POST',
            body: data
        }).then(res => res.text())
            .then((res) => {
                const cardOverviewElem = document.getElementById("card-overview");

                cardOverviewElem.innerHTML = res;

                // @ts-ignore
                htmx.process(cardOverviewElem);
                this.loading = false;
            });
    },

    resetFilters() {
        this.filters.forEach((filter: Filter) => {
            filter.filterItems.forEach(item => {
                if (item.option === ItemOption.None) { return false; }
                this.removeFilter(item);
            })
        });
    },

    removeFilter(filterItem: FilterItem) {
        filterItem.option = ItemOption.None;
        if (filterItem.element) {
            filterItem.element.checked = false;
        }

        this.updateUrl();
        this.updateOverview();
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

        if (this.pageNumber){
            url.searchParams.append("page", this.pageNumber);
        }

        history.replaceState({}, null, url);
    }
});

interface Config {
    changeUrl: boolean;
}

interface Filter {
    key: string,
    filterItems: FilterItem[],
    elementLess: boolean
}

interface FilterItem {
    value: string,
    label: string,
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