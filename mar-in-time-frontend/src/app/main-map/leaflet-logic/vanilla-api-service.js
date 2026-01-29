export async function fetchGet(url) {
  try {
    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Response status: ${response.status}`);
    }
    const result = await response.json();
    
    return result;
  } 
  catch (e) {
    console.error(e);
  }
}