function addnewtrack(params) {
    if (params.recId == null)
        return false;

    return Save("tracks", params);
}

function findtrackbyid(params) {
    return FindById("tracks", params.recId);
}
