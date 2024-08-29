export default () => {
    document.addEventListener("click", (ev: MouseEvent) => {
        const target = (ev.target as HTMLElement).closest(".js-open-modal");
        if (!target){
            return;
        }

        const modalId = target.getAttribute("js-modal");
        
        (document.getElementById(modalId) as HTMLDialogElement).showModal();

        ev.preventDefault();
    });
}