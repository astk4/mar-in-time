let crtAbortController = null;
var prevMapCenter = null,
    prevViewportWidth = null; // tracking viewport height is irrelevant because it depends on latitude
const minMoveThreshold = 0.03; // percentage of map dimensions

var streamResidue = "";

export async function consumeStreamedResponse(response, funcPerObject) {
    const reader = response.body.getReader();
    const decoder = new TextDecoder(); // for converting bytes to text

    while (true) {
        const { done, value } = await reader.read();

        if (done) {
            console.log('Stream completed');
            break;
        }

        const chunk = streamResidue + decoder.decode(value, { stream: true });
        const linesInChunk = chunk.split('\n');

        streamResidue = linesInChunk.pop();

        if (funcPerObject) 
        {
            for (const line of linesInChunk) 
            {
                if (line[line.length - 1] === '}') 
                {
                    funcPerObject(line);
                }
            }
        }
        else {
            console.log('Received chunk:', chunk);
        }
    }
}

export async function fetchGetRequestAbortable(url) {
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
        if (response.status === 204) {
            console.log('Zoomed in within the same tier, no need to fetch new data');
            return null; // nothing to fetch
        }

        return response;
    }
    catch (err) {
        if (err.name === 'AbortError') {
            console.log('Fetch aborted');
            return null; // request canceled
        }
        console.error(err);
    }
}

export function validateMove(bbox, centerNow) 
{
    let viewportWidth = Math.abs(bbox.East - bbox.West),
        viewportHeight = Math.abs(bbox.North - bbox.South);
        
    if (prevMapCenter == null) 
    {
        prevMapCenter = centerNow;
        prevViewportWidth = viewportWidth;
        return true;
    }

    if (Math.abs(viewportWidth - prevViewportWidth) > 0.000001) 
    {
        prevViewportWidth = viewportWidth;
        console.log('Viewport size changed, probably it was a zoom, so data was already fetched');
        return false;
    }

    let dx = Math.abs(centerNow.lng - prevMapCenter.lng),
        dy = Math.abs(centerNow.lat - prevMapCenter.lat);

    if (dx / viewportWidth >= minMoveThreshold || dy / viewportHeight >= minMoveThreshold) 
    {
        prevMapCenter = centerNow;    
        prevViewportWidth = viewportWidth;
        return true;
    }

    console.log('Moved too little, no need to fetch new data');
    return false;
}
