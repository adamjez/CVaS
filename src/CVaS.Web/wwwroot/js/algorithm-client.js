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
        $("#request-message").delay(5000).slideUp(500, 0);
    };

    var createLabelForStatus = function (status) {
        var label = 'info';

        if (status.toUpperCase() === 'success'.toUpperCase())
            label = 'success';
        else if (status.toUpperCase() === 'timeouted'.toUpperCase())
            label = 'warning';
        else
            label = 'danger';

        return '<span class="label label-' + label +'">' + status + '</span>'
    }

    var checkForRun = function (runId, divElement, divElementImgOut) {
        $.ajax({
            type: 'GET',
            url: '/runs/' + runId,
            timeout: 0,
            headers: {
                "accept": "application/json"
            },
            success: function (data) {
                showResult(data.id, data.status, data.duration, data.stdOut,
                    data.stdErr, data.zip, divElement, divElementImgOut);
            }
        });
    }

    var notFinishedResult = function (runId, element, elementImgOut) {
        var button = document.createElement('button');
        button.innerHTML = 'Check Again (Checking again automatically)';

        element.append('<div class="progress"><div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0" aria-valuemax="100"></div ></div >');

        var timeleft = 6;
        var downloadTimer = setInterval(function () {
            if (timeleft <= 0) {
                clearInterval(downloadTimer);
                checkForRun(runId, element, elementImgOut);
            }
            else {
                var currentValue = 100 - (--timeleft) * 20;
                element.find('.progress-bar').css('width', currentValue + '%').attr('aria-valuenow', currentValue);
            }
        }, 1000);

        button.onclick = function () {
            clearInterval(downloadTimer);
            checkForRun(runId, element, elementImgOut);
            return false;
        };

        element.append(button);
    }

    var showImageFromZip = function (element, zipFileUrl) {
        zip.createReader(new zip.HttpReader(zipFileUrl), function (zipReader) {
            zipReader.getEntries(function (entries) {
                // get data from the first file
                entries.forEach(function (entry) {
                    entry.getData(new zip.BlobWriter("application/octet-stream"), function (data) {
                        // close the reader and calls callback function with uncompressed data as parameter
                        zipReader.close();

                        var imgElementInput = element.find('div.qq-upload-input-image');
                        imgElementInput.removeClass('qq-image-full-width');

                        var imgElementOutput = element.find('div.qq-upload-output-image');
                        imgElementOutput.addClass('qq-output-image-added');

                        var imgElement = element.find('div.resultImg');
                        imgElement.prepend('<a class="result-preview" href="#"><img class="result-preview" src="' + URL.createObjectURL(data) + '"/></a>');
                        imgElement.find("a.result-preview").click(function (ev) {
                            $.featherlight('<img src="' + URL.createObjectURL(data) + '"/>');
                            ev.preventDefault();
                            return false;
                        });
                    });
                })
            });
        }, onerror);
    }

    var showResult = function (runId, status, duration, stdOut, stdErr, zipFile, element, elementImgOut) {
       
        // Clear inner html
        element.html("");

        element.append('<p>Id: ' + runId + '</p>');

        element.append('<p>Status: ' + createLabelForStatus(status) + '</p>');

        if (duration !== undefined) {
            element.append('<p>Duration: ' + (duration / 1000).toFixed(2) + ' s</p>');
        }

        if (status === "notFinished") {
            notFinishedResult(runId, element, elementImgOut);
            return;
        }

        if (stdOut !== undefined && stdOut.trim() !== '') {
            element.append('<p>StdOut: ' + stdOut + '</p>');
        }

        if (stdErr !== undefined && stdErr.trim() !== '') {
            element.append('<p>StdErr: ' + stdErr + '</p>');
        }

        if (zipFile !== undefined) {
            element.append('<p>Zip: <a href= "' + zipFile + '">download</a></p>');
            showImageFromZip(elementImgOut, zipFile);
        }
    };

    var runAlgorithm = function (filename, id, qqId) {
        var result = $('li[qq-file-id=\'' + qqId + '\']');
        var loader = result.find('.loader');
        var resultDiv = result.find('div.result');
        var resultImgDiv = result.find('div.qq-upload-images');

        var requestBody = _this.createRequestBodyCallback(id);
        // if callback returns null, we dont run algorithm
        // Most common use is that client want more images uploaded
        if (requestBody === null) {
            resultDiv.append('<p><b>Upload next image to trigger algorithm run</b></p>');
            return;
        }

        var jsonBody = JSON.stringify(requestBody);

        loader.show();
        showMessage('POST', _this.algorithmEndpoint, jsonBody);

        $.ajax({
            type: 'POST',
            url: _this.algorithmEndpoint,
            data: jsonBody,
            timeout: 0,
            headers: {
                "accept": "application/json"
            },
            contentType: 'application/json; charset=utf-8',
            dataType: "json",
            success: function (data) {
                loader.hide();
                showResult(data.id, data.status, data.duration, data.stdOut,
                    data.stdErr, data.zip, resultDiv, resultImgDiv);
            },
            complete: function () {
                loader.hide();
            },
            error: function () {
                resultDiv.append('<p><b>Something bad happened. Contact admin, please</b></p>')
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