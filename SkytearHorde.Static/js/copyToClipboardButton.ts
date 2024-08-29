export default () => {

    document.querySelectorAll(".js-copy-clipboard-button").forEach(element => {
        element.addEventListener("click", (event) => {
            const targetElement = event.currentTarget as HTMLAnchorElement;
            const url = targetElement.href;

            fetch(url)
                .then((response) => response.text())
                .then((response) => {
                    navigator.clipboard.writeText(response);
                    targetElement.getElementsByTagName("p")[0].textContent = "Copied to clipboard";
                }
            );

            event.preventDefault();
        });
    });
}