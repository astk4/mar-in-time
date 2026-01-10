let crtAbortController = null;
var prevMapCenter = null;
const minMoveThreshold = 0.03; // percentage of map dimensions

export async function fetchGetAbortable(url) {
    if (crtAbortController) {
        crtAbortController.abort();
    }
    crtAbortController = new AbortController();

    try {
        const response = await fetch(url, {
            signal: crtAbortController.signal
        });
        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }
        const result = await response.json();

        return result;
    }
    catch (err) {
        if (err.name === 'AbortError') {
            console.log('Fetch aborted');
            return null; // request canceled
        }
        console.error(err);
    }
}

export function validateMove(bbox, centerNow) {

    if (prevMapCenter == null) {
        prevMapCenter = centerNow;
        return true;
    }

    let viewportWidth = Math.abs(bbox.East - bbox.West),
        viewportHeight = Math.abs(bbox.North - bbox.South);

    let dx = Math.abs(centerNow.lng - prevMapCenter.lng),
        dy = Math.abs(centerNow.lat - prevMapCenter.lat);

    if (dx / viewportWidth >= minMoveThreshold || dy / viewportHeight >= minMoveThreshold) {
        prevMapCenter = centerNow;
        return true;
    }

    return false;
}
