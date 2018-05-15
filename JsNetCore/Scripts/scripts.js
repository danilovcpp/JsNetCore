var devices = {
	save: function (params) {

		if (params.recid == null)
			return false;

		var deviceJson = devices.findbyrecid("devices", params);
		if (deviceJson != "")
			return false;

		if (params.recname == null)
			params.recname = "New unknow device";

		var userJson = users.findbyrecid(params);
		var user = null;

		if (userJson != "")
			user = JSON.parse(userJson);

		if (user == null) {
			user = { recid: params.recid, recname: params.recname };
		} else {
			user["recname"] = params.recname;
		}

		var uresult = users.save(user);
		params.userid = user.recid;
		var dresult = Insert("devices", params);

		return (uresult && dresult);
	},
	findbyrecid: function (params) {
		if (params.recid == null)
			return false;

		return FindByRecid("devices", params.recid);
	}
}

var histories = {
	save: function (params) {

		if (devices.findbyrecid({ recid: params.deviceid }) == "" || tracks.findbyrecid({ recid: params.trackid }) == "")
			return false;

		if (params.recid == null)
			return false;

		var historyTemp = null;
		var historyTempJson = histories.findbyrecid(params);

		if (historyTempJson != "")
			historyTemp = JSON.parse(historyTempJson);

		if (historyTemp != null) {
			var count = historyTemp["countsend"];
			UpdateByRecid("histories", historyTemp.recid, { countsend: count == null ? 0 : ++count });
		} else {
			historyTemp = {
				recid: params.recid,
				trackid: params.trackid,
				deviceid: params.deviceid,
				countsend: 1,
				islisten: params.islisten,
				lastlisten: params.lastlisten
			}
		}

		Insert("histories", historyTemp);

		var ratingJson = ratings.findbyparams({ userid: params.userid, trackid: params.trackid });
		var rating = null;
		var dbresult = false;

		if (ratingJson != "") {
			rating = JSON.parse(ratingJson);
		}

		if (rating != null) {
			dbresult = ratings.update({ recid: rating[0].recid, lastlisten: params.lastlisten, ratingsum: Number(rating[0].ratingsum) + Number(params.islisten) });
		} else {
			dbresult = ratings.save({
				userid: params.userid,
				trackid: params.trackid,
				lastlisten: params.lastlisten,
				ratingsum: params.islisten
			});
		}

		//TODO: ratio update

		return dbresult;
	},
	findbyrecid: function (params) {
		if (params.recid == null)
			return false;

		return FindByRecid("histories", params.recid);
	}
}

var tracks = {
	next: function (params) {

		if (params.mediatype == "audiobook") {

			if (params.chapter == null) {
				params.chapter = 1;
				var listOfAudiobook = FindByParams("tracks", params);

				if (listOfAudiobook == "")
					return null;

				var jsonAudiobooks = JSON.parse(listOfAudiobook);
				return jsonAudiobooks[0 + Math.floor(Math.random() * (jsonAudiobooks.length - 0))].recid;
			}

			var chapter = Number(params.chapter) + 1;
			params.chapter = chapter;

			var listAudiobookFile = FindByParams("tracks", params);
			if (listAudiobookFile == "")
				return null;

			return JSON.parse(listAudiobookFile)[0].recid;
		}

		return ExecSqlProcedure("getnexttrackid", { "i_deviceid": params.deviceid });
	},
	save: function (params) {
		if (params == null)
			return false;

		return Insert("tracks", params);
	},
	findbyrecid: function (params) {
		if (params.recid == null)
			return "";

		var trackJson = FindByRecid("tracks", params.recid);
		if (trackJson == "")
			return "";

		tracks.fields = JSON.parse(trackJson);
		tracks.fileMp3.value = tracks.fields.path;

		return trackJson;
	},
	findbydevice: function (params) {
		return FindByParams("tracks", params);
	},
	fields: {},
	fileMp3: {
		value: "",
		getBlob: function () {
			return LoadFile(tracks.fileMp3.value);
		},
		upload: function (params) {
			return UploadFile(params);
		}
	},
	listen: function (params) {
		tracks.findbyrecid(params);
		return tracks.fileMp3.getBlob();
	},

	upload: function (params) {
		var filePath = UploadFile(params.content, params.recname);

		var audiofileinfo = {
			recid: params.recid,
			mediatype: params.mediatype,
			chapter: params.chapter,
			recname: params.recname,
			ownerrecid: params.ownerrecid,
			path: filePath,
			state: 1
		};

		Insert("tracks", audiofileinfo);
	},
	downloadaudiobook: function (params) {
		var downloadResult = DownloadFile(params.downloadurl);

		if (downloadResult.Success == false)
			return;

		return downloadResult.Data;

	},
	saveaudiobook: function (params) {

		//add table for save history id, name, url, isFull
		//todo download audiobook, return local path

		var path = DownloadFile(params.downloadurl);
		var files = UnpackArchive(path);

		if (files.length == 0)
			return;

		var ownerrecid = null;
		var chapter = 0;

		files.Data.forEach(function (file) {

			if (file.FileName.split('.').pop() != "mp3") {
				return;
			}

			chapter++;

			if (chapter == 1) {
				ownerrecid = file.LocalFileName;
			}

			var tableParams = {
				recid: file.LocalFileName,
				recname: file.FileName,
				recdescription: params.audiobookname,
				mediatype: "audiobook",
				path: file.Path,
				chapter: chapter,
				ownerrecid: ownerrecid,
				audiobookurl: params.audiobookurl
			};

			Insert("tracks", tableParams);
		});
	}
}

var users = {
	save: function (params) {
		if (params == null)
			return false;

		return Insert("users", params);
	},
	findbyrecid: function (params) {
		if (params.recid == null)
			return false;

		return FindByRecid("users", params.recid);
	}
}

var ratings = {
	save: function (params) {
		if (params == null)
			return false;

		return Insert("ratings", params);
	},
	update: function (params) {
		if (params == null)
			return false;

		return UpdateByRecid("ratings", params.recid, params);
	},
	findbyparams: function (params) {
		if (params == null)
			return false;

		return FindByParams("ratings", params);
	},
	findbyrecid: function (recid) {
		if (recid == null)
			return false;

		return FindByRecid("ratings", recid);
	}
}
