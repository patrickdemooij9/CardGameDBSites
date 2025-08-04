(function () {
    "use strict";

    function ImporterController(editorState) {

        var vm = this;

        console.log("Test");

        vm.importFile = "";
        vm.test = "hello";

        vm.doImport = doImport;
        vm.doImportImages = doImportImages;
        vm.doImportJson = doImportJson;

        function doImport() {
            var formData = new FormData();
            formData.append("file", $("#import-file")[0].files[0]);
            formData.append("nodeId", editorState.current.id);
            $.ajax({
                url: '/umbraco/backoffice/importer/importer/import',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false
            });
        }

        function doImportImages() {
            var formData = new FormData();
            formData.append("file", $("#import-images")[0].files[0]);
            formData.append("nodeId", editorState.current.id);
            $.ajax({
                url: '/umbraco/backoffice/importer/importer/importImages',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false
            });
        }

        function doImportJson() {
            var formData = new FormData();
            formData.append("file", $("#import-json")[0].files[0]);
            formData.append("nodeId", editorState.current.id);
            formData.append("setId", $("#import-json-set-id").val());
            $.ajax({
                url: '/umbraco/backoffice/importer/importer/ImportJsonFiles',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false
            });
        }
    }

    angular.module("umbraco").controller("Importer.Controller", ImporterController);
})();