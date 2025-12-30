export const portPopupTemplate = (data) => 
    `<div class="port-popup">
        <h5>${data.fullId} ${data.name}</h5>
        <p>${data.latitude}, ${data.longitude}</p>
        <p>${data.country}</p>
        <p><b>Routes:</b></p>
        ${data.routes.map(r => `<p>#${r}</p>`).join('')}
    </div>`;