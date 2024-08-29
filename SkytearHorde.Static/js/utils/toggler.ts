export default () => {
    const togglers = Array.from<HTMLInputElement>(document.querySelectorAll("[data-toggle]"));
    togglers.forEach((element) => {
        const toggleElements = element.dataset.toggle;
        element.addEventListener("input", () => {
            const toggleElements = document.querySelectorAll<HTMLElement>(element.dataset.toggle);
            toggleElements.forEach((el) => el.style.display = el.style.display == 'none' ? '' : 'none');
        })
    })
}