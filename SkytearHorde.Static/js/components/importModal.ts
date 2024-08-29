export default () => ({
    step: 0,
    loading: false,
    file: undefined,
    errorMessage: undefined,

    submitForm(event: SubmitEvent) {
        const target = event.target as HTMLFormElement;

        event.preventDefault();

        const data = new FormData(target);

        this.loading = true;
        this.errorMessage = undefined;
        fetch(target.action, {
            method: 'POST',
            body: data
        }).then((resp) => {
            if (resp.ok) {
                target.reset();

                this.loading = false;
                this.step = 1;
            } else {
                this.loading = false;
                resp.text().then((text) => {
                    this.errorMessage = text;
                });
            }
        }, (error) => {
            console.log(error);
            this.loading = false;
        });

        return false;
    },
})