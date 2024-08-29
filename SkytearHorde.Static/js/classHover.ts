export default () => {
    document.querySelectorAll(".js-class-hover").forEach(element => {
        element.addEventListener("mouseenter", (event) => {
            const targetElement = event.currentTarget as HTMLAnchorElement;
            const classToAdd = targetElement.getAttribute("js-class-hover");

            targetElement.classList.add(classToAdd);
        });
        element.addEventListener("mouseleave", (event) => {
            const targetElement = event.currentTarget as HTMLAnchorElement;
            const classToAdd = targetElement.getAttribute("js-class-hover");

            targetElement.classList.remove(classToAdd);
        })
    })
}