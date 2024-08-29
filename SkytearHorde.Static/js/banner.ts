const observer = new IntersectionObserver((entries: IntersectionObserverEntry[]) => {
    entries.forEach(entry => {
        if (entry.isIntersecting){
            observer.unobserve(entry.target);

            loadBanner(entry.target);
        }
    })
}, {
    threshold: [0.5]
});

document.querySelectorAll("[data-banner]").forEach((element: HTMLElement) => {
    observer.observe(element);
});

function loadBanner(element: Element){
    fetch("/umbraco/api/banner/displaybanner")
        .then((resp) => resp.json())
        .then((resp) => element.innerHTML = resp.value);
}