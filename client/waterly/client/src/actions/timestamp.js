export const renderTime = (timestamp) => { 
    const options = {
        year: 'numeric', month: 'numeric', day: 'numeric',
        hour: 'numeric', minute: 'numeric', second: 'numeric',
        hour12: false,
        timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone 
    };
    if(timestamp > 0){
        // to specify options but use the browser's default locale, use 'default'
        return new Intl.DateTimeFormat('default', options).format(timestamp*1000);
    } else {
        return null;
    }

}