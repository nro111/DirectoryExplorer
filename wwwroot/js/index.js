$(document).ready(function () {
    $('.modal').each(function () {
        M.Modal.init(this);
    });

    // Check for deep link
    var params = new URLSearchParams(window.location.search);
    var path = params.get("path") || "";

    if (path !== "") {
        loadDirectoryGrid(path);
    } else {
        // Fetch origin path on page load
        $.ajax({
            url: '/FileSystem/api/home',
            method: 'GET',
            success: function (response) {
                const origin = response.home || '';
                $('#txtOriginPath').val(origin);
                loadDirectoryGrid(origin);
            },
            error: function () {
                M.toast({ html: 'Error fetching origin path.' });
            }
        });
    }

    // Set new origin path
    $('#btnSetOrigin').on('click', function () {
        var newOrigin = $('#txtOriginPath').val().trim();
        if (!newOrigin) {
            M.toast({ html: 'Origin path cannot be empty!' });
            return;
        }

        $.ajax({
            url: '/FileSystem/api/home',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(newOrigin),
            success: function () {
                M.toast({ html: 'Origin path updated to: ' + newOrigin });
                loadDirectoryGrid('');
            },
            error: function () {
                M.toast({ html: 'Error updating origin path.' });
            }
        });
    });

    $('#btnResetToOrigin').on('click', function () {
        loadDirectoryGrid($('#txtOriginPath').val().trim());
    })

    // Search
    $('#btnSearch').on('click', function () {
        var query = $('#txtQuery').val().trim();
        if (!query) {
            M.toast({ html: 'Enter a search term' });
            return;
        }

        $.ajax({
            url: '/FileSystem/api/search',
            method: 'GET',
            data: { query: query },
            success: function (response) {
                renderGrid(response);
            },
            error: function () {
                M.toast({ html: 'Search failed' });
            }
        });
    });

    // Upload
    $('#uploadSubmit').on('click', function (e) {
        e.preventDefault();

        var file = $('#fileInput')[0].files[0];
        var path = $('#uploadPath').val();

        if (!file || !path) {
            alert('Please choose a file and provide a path.');
            return;
        }

        var formData = new FormData();
        formData.append('file', file);
        formData.append('path', path);

        $.ajax({
            url: '/FileSystem/api/upload',
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function () {
                M.toast({ html: 'File uploaded successfully!' });
                $('#uploadForm')[0].reset();
                $('#upload').modal('close');
                loadDirectoryGrid(path);
            },
            error: function () {
                M.toast({ html: 'Upload failed' });
            }
        });
    });

    // When delete icon is clicked, copy data-* into the modal confirm button
    $(document).on('click', '.delete-trigger', function () {
        const filename = $(this).data('filename');
        const foldername = $(this).data('foldername');

        const confirmButton = $('#delete .confirm-delete');
        confirmButton.removeData('filename').removeData('foldername');

        if (filename) {
            confirmButton.data('filename', filename);
        } else if (foldername) {
            confirmButton.data('foldername', foldername);
        }
    });

    // Handle delete
    $(document).on('click', '#delete .confirm-delete', function () {
        const filename = $(this).data('filename');
        const foldername = $(this).data('foldername');
        let url = '';

        if (filename) {
            url = `/FileSystem/api/delete/file/${encodeURIComponent(filename)}`;
        } else if (foldername) {
            url = `/FileSystem/api/delete/folder/${encodeURIComponent(foldername)}`;
        }

        $.ajax({
            url: url,
            type: 'DELETE',
            success: function () {
                M.toast({ html: 'Deleted successfully' });
                loadDirectoryGrid('');
            },
            error: function () {
                M.toast({ html: 'Delete failed' });
            }
        });
    });

    // Create folder
    $(document).on('click', '#createFolder .confirm-folder-create', function () {
        const folderName = $('#folder_name').val().trim();
        if (!folderName) {
            M.toast({ html: 'Folder name required' });
            return;
        }

        const fullPath = (currentPath ? currentPath + "\\" : "") + folderName;

        $.ajax({
            url: `/FileSystem/api/create/folder/${encodeURIComponent(fullPath)}`,
            type: 'POST',
            success: function () {
                M.toast({ html: 'Folder created' });
                $('#createFolder').modal('close');
                loadDirectoryGrid(currentPath);
            },
            error: function () {
                M.toast({ html: 'Create folder failed' });
            }
        });
    });

    // Download
    $(document).on('click', '.download-btn', function () {
        var path = $(this).data('path');
        window.location = '/FileSystem/api/download?path=' + encodeURIComponent(path);
    });

    // Browse when clicking row
    $(document).on('click', '#tblDataTable tbody tr', function () {
        var folderPath = $(this).find('td:nth-child(3)').text();
        loadDirectoryGrid(folderPath);
    });
});

// Track current path
let currentPath = "";

window.onpopstate = function (event) {
    var path = (event.state && event.state.path) ? event.state.path : "";
    loadDirectoryGrid(path);
};

function updateBreadcrumb(path) {
    var breadcrumbContainer = $('.nav-wrapper .col.s12');
    breadcrumbContainer.empty();

    var fullPath = path || "";
    var parts = fullPath.split(/[\\/]/).filter(p => p.length > 0);

    // Update URL for deep linking
    var newUrl = window.location.pathname + (fullPath ? "?path=" + encodeURIComponent(fullPath) : "");
    window.history.pushState({ path: fullPath }, "", newUrl);

    // Root crumb
    var homeCrumb = $('<a href="#!" class="breadcrumb">Home</a>');
    homeCrumb.on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        loadDirectoryGrid("");
    });
    breadcrumbContainer.append(homeCrumb);

    // Build subpath crumbs
    var cumulativeParts = [];
    parts.forEach(function (part) {
        cumulativeParts.push(part);

        // Build cumulative path with proper separator for server
        var cumulativePath = cumulativeParts.join("\\");

        var crumb = $('<a href="#!" class="breadcrumb">' + part + '</a>');
        crumb.on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            loadDirectoryGrid(cumulativePath);
        });
        breadcrumbContainer.append(crumb);
    });
}

// Function to load root or specific directory
function loadDirectoryGrid(path) {
    currentPath = path;
    $.ajax({
        url: '/FileSystem/api/browse',
        method: 'GET',
        data: { path: path },
        success: function (response) {
            renderGrid(response);
            updateBreadcrumb(path);
        },
        error: function () {
            M.toast({ html: 'Failed to load directory' });
        }
    });
}

// Render function to build table
function renderGrid(itemResult) {
    var tableBody = $('#tblDataTable tbody');
    tableBody.empty();
    $('#itemCount').empty();
    $('#itemCount').append('<h4>Number of files and folders: ' + itemResult.count + '</h4>');

    for (let i = 0; i < itemResult.count; i++) {
        const item = itemResult.items[i];
        var row = '<tr>' +
            '<td>' + item.name + '</td>' +
            '<td>' + item.type + '</td>' +
            '<td>' + item.path + '</td>' +
            '<td>' + item.size + ' Bytes</td>' +
            '<td>';

        if (item.type.toLowerCase() === 'file') {
            row += '<i class="small material-icons clickable download-btn" data-path="' + item.path + '">file_download</i>';
            row += '<a class="modal-trigger delete-trigger" href="#delete" data-filename="' + item.path + '">' +
                '<i class="small material-icons clickable">delete</i></a>';
        } else {
            row += '<a class="modal-trigger delete-trigger" href="#delete" data-foldername="' + item.path + '">' +
                '<i class="small material-icons clickable">delete</i></a>';
        }

        row += '</td></tr>';
        tableBody.append(row);
    }
}
