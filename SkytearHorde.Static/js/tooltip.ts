export default () => {
    document.querySelectorAll(".tooltip-starter").forEach((element: HTMLElement) => {
        const tooltips = element.querySelectorAll(".tooltip");
        const parentBounding = element.getBoundingClientRect();

        tooltips.forEach(item => {
            const bottomXTooltip = 300 + parentBounding.y;
            if (bottomXTooltip >= window.innerHeight) {
                item.classList.add("top");
            }
        })
        element.addEventListener("mouseenter", () => {
            tooltips.forEach(item => {
                item.classList.add("shown");
            });
        });

        element.addEventListener("mouseleave", () => {
            tooltips.forEach(item => item.classList.remove("shown"));
        })
    });

    document.querySelectorAll(".tooltip-container").forEach((element: HTMLElement) => {
        element.addEventListener("mouseenter", (event: MouseEvent) => {
            console.log(event);
        }, true);
    });
}