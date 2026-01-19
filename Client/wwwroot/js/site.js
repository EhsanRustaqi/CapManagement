window.downloadFile = (fileName, byteBase64) => {
    try {
        const link = document.createElement("a");
        link.href = "data:application/pdf;base64," + byteBase64;
        link.download = fileName || "download.pdf";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    } catch (e) {
        console.error("downloadFile failed", e);
    }
};




// Used by settlement PDF download

//window.downloadPdfFile = (fileName, contentType, byteBase64) => {
//    try {
//        const link = document.createElement("a");
//        link.href = `data:${contentType};base64,${byteBase64}`;
//        link.download = fileName || "download.pdf";
//        document.body.appendChild(link);
//        link.click();
//        document.body.removeChild(link);
//        console.log(`PDF downloaded: ${fileName}`);
//    } catch (e) {
//        console.error("downloadPdfFile failed", e);
//        alert("PDF download failed. Please try again.");
//    }
//};
window.downloadPdfFile = (fileName, contentType, byteBase64) => {
    try {
        // decode base64 to raw binary
        const byteCharacters = atob(byteBase64);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);

        // create a blob from binary data
        const blob = new Blob([byteArray], { type: contentType });

        // create object URL for blob
        const blobUrl = URL.createObjectURL(blob);

        // create & click link
        const link = document.createElement("a");
        link.href = blobUrl;
        link.download = fileName || "download.pdf";
        document.body.appendChild(link);
        link.click();

        // cleanup
        document.body.removeChild(link);
        URL.revokeObjectURL(blobUrl);

        console.log(`PDF downloaded: ${fileName}`);
    } catch (e) {
        console.error("downloadPdfFile failed", e);
        alert("PDF download failed. Please try again.");
    }
};


