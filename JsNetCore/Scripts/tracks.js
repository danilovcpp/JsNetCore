function addnewtrack(params) {
    if (params.recId == null)
        return false;

    return Insert("tracks", params);
}

function findtrackbyid(params) {
    return FindById("tracks", params.recId);
}
