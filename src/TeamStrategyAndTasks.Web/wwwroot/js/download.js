/**
 * Triggers a file download entirely in the browser.
 * Called from Blazor via IJSRuntime.InvokeVoidAsync("downloadFile", filename, mimeType, content).
 */
window.downloadFile = function (filename, mimeType, content) {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
