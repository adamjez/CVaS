"use strict";
function AlgorithmClient(algorithmEndpoint, createRequestBodyCallback) {
    var _this = this;
    this.algorithmEndpoint = algorithmEndpoint;
    this.createRequestBodyCallback = createRequestBodyCallback;

    zip.workerScriptsPath = "/lib/zipjs/WebContent/";

    var showMessage = function (method, location, body) {
        $("#body-request-message").html("<p>" + method + " " + location + "</p><p>BODY: " + body + "</p>");
        $("#request-message").show();
        $("#request-message").alert();
        $("#request-message").delay(4000).slideUp(500, 0);
    };

    var showResult = function (runId, status, stdOut, stdErr, zipFile, element) {
        element.append('<p>RunId: ' + runId + '</p>');
        element.append('<p>Status: ' + status + '</p>');

        if (status === "notFinished") {
            // ToDo: add button to check for run again
            return;
        }

        if (stdOut !== undefined && stdOut.trim() !== '') {
            element.append('<p>StdOut: ' + stdOut + '</p>');
        }

        if (stdErr !== undefined && stdErr.trim() !== '') {
            element.append('<p>StdErr: ' + stdErr + '</p>');
        }

        if (zipFile === undefined) {
            return;
        }

        element.append('<p>Zip: <a href= "' + zipFile + '"> ' + zipFile + '</a></p>');

        zip.createReader(new zip.HttpReader(zipFile), function (zipReader) {
            zipReader.getEntries(function (entries) {
                // get data from the first file
                var entry = entries[0];
                entry.getData(new zip.BlobWriter("image/jpg"), function (data) {
                    // close the reader and calls callback function with uncompressed data as parameter
                    zipReader.close();

                    element.prepend('<img class="result-preview" src="' + URL.createObjectURL(data) + '"/>');
                });
            });
        }, onerror);
    };

    var runAlgorithm = function (filename, id, qqId) {
        var result = $('li[qq-file-id=\'' + qqId + '\']');
        var loader = result.find('.loader');
        loader.show();

        var requestBody = _this.createRequestBodyCallback(id);
        var jsonBody = JSON.stringify(requestBody);

        showMessage("POST", _this.algorithmEndpoint, jsonBody);

        $.ajax({
            type: 'POST',
            url: _this.algorithmEndpoint,
            data: jsonBody,
            timeout: 0,
            headers: {
                "accept": "application/json"
            },
            contentType: 'application/json',
            success: function (data) {
                loader.hide();
                showResult(data.runId, data.status, data.stdOut,
                    data.stdErr, data.zip, result.find('div.result'));
            }
        });
    };

    $('#fine-uploader-gallery')
        .fineUploader({
            template: 'qq-template-gallery',
            request: {
                endpoint: '/files'
            },
            deleteFile: {
                enabled: false,
                endpoint: '/files'
            },
            thumbnails: {
                placeholders: {
                    waitingPath: '/lib/fine-uploader/dist/placeholders/waiting-generic.png',
                    notAvailablePath: '/lib/fine-uploader/dist/placeholders/not_available-generic.png'
                }
            },
            validation: {
                allowedExtensions: ['jpeg', 'jpg', 'gif', 'png', 'bmp']
            },
            callbacks: {
                onComplete: function (qqId, name, responseJson) {
                    responseJson.ids.forEach(function (id) {
                        runAlgorithm(name, id, qqId);
                    });
                }
            }
        });
};