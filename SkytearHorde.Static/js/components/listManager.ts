export default () => {
  document.querySelectorAll("[js-list]").forEach((elem: HTMLElement) => {
    let currentValue =
      elem.getAttribute("js-list-value") == "True" ? true : false;
    const list = elem.getAttribute("js-list");
    const setId = elem.getAttribute("js-set");
    elem.addEventListener("click", () => {
      fetch("/umbraco/api/collection/updateSetInList", {
        body: JSON.stringify({
          setId: setId,
          listName: list,
          value: currentValue,
        }),
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
      })
      .then((resp) => resp.json())
      .then((resp) => {
        elem.innerText = resp.text;
        currentValue = !currentValue;
      });
    });
  });
};
