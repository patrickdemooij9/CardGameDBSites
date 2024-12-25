import { arrow, computePosition, flip, offset, shift } from "@floating-ui/dom";

export default () => {
  document
    .querySelectorAll(".anchor-source")
    .forEach((element: HTMLElement) => {
      const targetId = element.getAttribute("data-anchor-target-id");
      const target = document.querySelector("#" + targetId) as HTMLElement;

      if (!target) {
        return;
      }

      element.addEventListener("click", open);

      function open(event: MouseEvent) {
        event.stopPropagation();
    
        target.classList.remove("hidden");
        update(element, target);
    
        element.removeEventListener("click", open);
        element.addEventListener("click", close);
        window.addEventListener("click", tryClose);
      }
    
      function tryClose(event: MouseEvent) {
        const clickTarget = event.target as HTMLElement;
        if (!clickTarget.closest(target.id)){
            close();
        }
      }
    
      function close() {
        target.classList.add("hidden");
    
        element.addEventListener("click", open);
        element.removeEventListener("click", close);
        window.removeEventListener("click", tryClose);
      }
    });

  function update(buttonElem: HTMLElement, targetElem: HTMLElement) {
    const arrowId = buttonElem.getAttribute("data-anchor-arrow-id");
    const arrowEl = document.querySelector("#" + arrowId) as HTMLElement;

    computePosition(buttonElem, targetElem, {
      placement: "top-start",
      middleware: [offset(6), flip(), shift(), arrow({ element: arrowEl })],
    }).then(({ x, y, placement, middlewareData }) => {
      Object.assign(targetElem.style, {
        left: `${x}px`,
        top: `${y}px`,
      });

      // Accessing the data
      const { x: arrowX, y: arrowY } = middlewareData.arrow;

      const staticSide = {
        top: "bottom",
        right: "left",
        bottom: "top",
        left: "right",
      }[placement.split("-")[0]];

      Object.assign(arrowEl.style, {
        left: arrowX != null ? `${arrowX}px` : "",
        top: arrowY != null ? `${arrowY}px` : "",
        right: "",
        bottom: "",
        [staticSide]: "-4px",
      });
    });
  }
};
