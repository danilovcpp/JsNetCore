function regnewdevice(params) {
    if (params.recId == null || params.recName == null) 
        return false;

    return Save("devices", params);
}

function finddevicebyid(params) {
    return FindById("devices", params.recId);
}
