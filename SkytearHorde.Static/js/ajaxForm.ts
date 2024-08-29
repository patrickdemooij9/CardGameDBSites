export default () => {
    document.addEventListener("submit", (event: SubmitEvent) => {
        const target = event.target as HTMLFormElement;
        if (!target || !target.classList.contains("js-ajax-form-submit")) { return; }

        event.preventDefault();

        var data = new FormData(target);
        data.append("isAjax", "true");
        if ((event.submitter as HTMLButtonElement)?.name){
            const buttonSubmitter = event.submitter as HTMLButtonElement;
            data.append(buttonSubmitter.name, buttonSubmitter.value);
        }

        fetch(target.action, {
            method: 'POST',
            body: data
        }).then(res => res.json())
            .then((res: AjaxResponse | AjaxResponse[]) => {
                target.reset();

                if (Array.isArray(res)) {
                    res.forEach(item => {
                        updateDomContent(item);
                    });
                } else {
                    updateDomContent(res);
                }
            });

        return false;
    });
}

function updateDomContent(res: AjaxResponse) {
    const element = document.getElementById(res.id);
    if (res.isInner) {
        element.innerHTML = res.content;
    }
    else {
        element.outerHTML = res.content;
    }
    
    // @ts-ignore
    htmx.process(element);
}

interface AjaxResponse {
    id: string;
    content: string;
    isInner: boolean;
}