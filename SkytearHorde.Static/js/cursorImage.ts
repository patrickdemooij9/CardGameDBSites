export default () => {
    const cursorImageElem = document.querySelector("#cursor-image") as HTMLElement;
    if (!cursorImageElem) { return; }

    const cursorImageElemRect = cursorImageElem.getBoundingClientRect();

    let currentSourceCursorImage: HTMLElement = null;
    window.addEventListener("mouseover", (event: MouseEvent) => {
        const sourceCursorImage = (event.target as HTMLElement).closest('.cursor-source') as HTMLElement;
        if (!sourceCursorImage) {
            deselectCurrentSource();
            return;
        }

        currentSourceCursorImage = sourceCursorImage;

        cursorImageElem.style.backgroundImage = `url(${sourceCursorImage.getAttribute("data-cursor-image")})`;

        window.addEventListener("mousemove", updateCursorImageLocation);
    });

    function deselectCurrentSource() {
        if (currentSourceCursorImage) {
            window.removeEventListener("mousemove", updateCursorImageLocation);
            cursorImageElem.style.backgroundImage = null;
            currentSourceCursorImage = null;
        }
    }

    function updateCursorImageLocation(event: MouseEvent) {
        if (cursorImageElemRect.height + event.clientY >= window.innerHeight) {
            cursorImageElem.style.top = `${event.pageY - cursorImageElemRect.height}px`;
        } else {
            cursorImageElem.style.top = `${event.pageY - 20}px`;
        }

        if (cursorImageElemRect.width + event.clientX + 40 >= window.innerWidth) {
            cursorImageElem.style.left = `${event.pageX - cursorImageElemRect.width - 20}px`;
        } else {
            cursorImageElem.style.left = `${event.pageX + 20}px`;
        }
    }
}