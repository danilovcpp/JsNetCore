///System
var tableModel = {
	insert: function (tableName, params) {
		var insertResult = Insert(tableName, params);
		if (insertResult.Success == false) {
			throw new Error(insertResult.Message);
		}

		return true;
	},
	update: function (tableName, params) {
		var updateResult = UpdateByRecid(tableName, params.recid, params);

		if (updateResult.Success == false) {
			throw new Error(updateResult.Message);
		}

		return true;
	},
	findbyrecid: function (tableName, recid) {
		var findResult = FindByRecid(tableName, recid);

		if (findResult.Success == false) {
			throw new Error(findResult.Message);
		}

		return findResult.Data != null ? JSON.parse(findResult.Data) : null;
	},
	findbyparams: function (tableName, params) {
		var findResult = FindByParams(tableName, params);

		if (findResult.Success == false) {
			throw new Error(findResult.Message);
		}

		return findResult.Data != null ? JSON.parse(findResult.Data) : null;
	},
	execprocedure: function (procedureName, params) {
		var result = ExecSqlProcedure("getnexttrackid", params);

		if (result.Success == false) {
			throw new Error(result.Message);
		}

		return result.Data;
	}
}
var fileModel = {
	download: function (url) {
		var downloadResult = DownloadFile(url);

		if (downloadResult.Success == false) {
			throw new Error(downloadResult.Message);
		}

		return downloadResult.Data;

	},
	upload: function (content, fileName) {
		var result = UploadFile(content, fileName);

		if (result.Success == false) {
			throw new Error(result.Message);
		}

		return result.Data;
	},
	getBlob: function (path) {
		var loadResult = LoadFile(path);

		if (loadResult.Success == false) {
			throw new Error(loadResult.Message);
		}

		return loadResult.Data;
	},
	unpack: function (path) {
		var result = UnpackArchive(path);

		if (result.Success == false) {
			throw new Error(result.Message);
		}

		return result.Data;
	}
}


///Analitics
//--------------------------------------------------------------------------------
function regnewdevice(params) {
	var device = tableModel.findbyrecid("devices", params.recid);
	if (device != null)
		return "Device already register";

	if (params.recname == null)
		params.recname = "New unknow device";

	var userInfo = tableModel.findbyrecid("users", params.recid);
	var user = null;

	if (userInfo == null) {
		user = { recid: params.recid, recname: params.recname };
	} else {
		user = { recid: userInfo.recid, recname: params.recname}
	}

	var uresult = tableModel.insert("users", user);
	params.userid = user.recid;
	var dresult = tableModel.insert("devices", params);

	return (uresult && dresult);
}

//--------------------------------------------------------------------------------
function showdeviceinfo(params) {
	return tableModel.findbyrecid("devices", params.recid);
}

//--------------------------------------------------------------------------------
function savehistory(params) {

	var tableName = "histories";
	var track = tableModel.findbyrecid("tracks", params.trackid);
	var device = tableModel.findbyrecid("devices", params.deviceid);

	if (track == null || device == null) {
		return false;
	}

	var history = tableModel.findbyrecid(tableName, params.recid);

	if (history != null) {
		var count = history.countsend;
		tableModel.update(tableName, { recid: history.recid, countsend: count == null ? 0 : ++count });
	} else {
		history = {
			recid: params.recid,
			trackid: params.trackid,
			deviceid: params.deviceid,
			countsend: 1,
			islisten: params.islisten,
			lastlisten: params.lastlisten
		}
	}

	tableModel.insert(tableName, history);

	var rating = tableModel.findbyparams("ratings", { userid: params.userid, trackid: params.trackid });
	var dbresult = null;

	if (rating != null) {
		dbresult = tableModel.update("ratings", { recid: rating[0].recid, lastlisten: params.lastlisten, ratingsum: Number(rating[0].ratingsum) + Number(params.islisten) });
	} else {
		dbresult = tableModel.insert("ratings", {
			userid: params.userid,
			trackid: params.trackid,
			lastlisten: params.lastlisten,
			ratingsum: params.islisten
		});
	}

	return dbresult;
}

//--------------------------------------------------------------------------------
function showhistoryinfo(params) {
	return tableModel.findbyrecid("histories", params.recid);
}

//--------------------------------------------------------------------------------
function nexttrack(params) {
	if (params.mediatype == "audiobook") {

		if (params.chapter == null) {
			params.chapter = 1;

			var tracks = tableModel.findbyparams("tracks", params);
			return tracks[0 + Math.floor(Math.random() * (jsonAudiobooks.length - 0))].recid;
		}

		var chapter = Number(params.chapter) + 1;
		params.chapter = chapter;

		var tracks = tableModel.findbyparams("tracks", params);

		return tracks[0].recid;
	}

	return tableModel.execprocedure("getnexttrackid", { "i_deviceid": params.deviceid });
}

//--------------------------------------------------------------------------------
function gettrack(params) {
	var track = tableModel.findbyrecid("tracks", params.recid);
	return fileModel.getBlob(track.path);
}

//--------------------------------------------------------------------------------
function showtrackinfo(params) {
	return tableModel.findbyrecid("tracks", params.recid);
}

//--------------------------------------------------------------------------------
function showtrackinfobydevice(params) {
	return tableModel.findbyparams("tracks", params)[0];
}

//--------------------------------------------------------------------------------
function uploadaudiofile(params) {

	var audiofileinfo = null;
	var path = fileModel.upload(params.content, params.recname);

	if (params.mediatype == "audiobook") {
		audiofileinfo = {
			recid: params.recid,
			mediatype: "audiobook",
			chapter: params.chapter,
			recname: params.recname,
			ownerrecid: params.ownerrecid,
			path: path,
			state: 1
		};
	} else if (params.mediatype == "track") {
		audiofileinfo = {
			recid: params.recid,
			mediatype: "track",
			recname: params.recname,
			path: path,
			state: 1
		};
	}

	return tableModel.insert("tracks", audiofileinfo);
}

//--------------------------------------------------------------------------------
function downloadaudiobook(params) {

	var downloadHistories = tableModel.findbyparams("downloadhistory", { url: params.url });
	var downloadHistory = null;

	if (downloadHistories != null) {
		downloadHistory = downloadHistories[0];
	}

	// Проверяем информацию из БД, если книга загружается менее 3-мин выходим из функции, если книга загружена выходим из функции
	if (downloadHistory != null) {
		// Если аудиокнига не была полностью скачена
		if (downloadHistory.isfull != 1) {

			// Если книга в процессе загрузки
			if (downloadHistory.inprocessing == 1) {
				let currentDate = new Date();
				let createDate = new Date(downloadHistory.reccreated);

				let delta = createDate - currentDate;

				let deltaDay = Math.abs(Math.round(delta / 1000 / 60 / 60 / 24));
				let deltaHours = Math.abs(currentDate.getUTCHours() - createDate.getHours());
				let deltaMinutes = Math.abs(currentDate.getUTCMinutes() - createDate.getMinutes());

				if (deltaDay == 0 && deltaHours == 0 && deltaMinutes <= 10) {
					return "Audiobook in process downloading"; // Книга загружается менее 10 мин, выходим
				} else {
					// Книга загружается слишком долго стоит повторить попытку
					tableModel.update("downloadhistory", {
						recid: downloadHistory,
						reccreated: new Date(),
						isfull: 0,
						inprocessing: 1
					});
				}
			}
		} else {
			return "Audiobook already downloaded";
		}
	}
	else {
		//В БД нет информации по книге, добавляем и приступаем к загрузке
		tableModel.insert("downloadhistory", {
			recname: params.name,
			reccreated: new Date(),
			url: params.url,
			mediatype: "audiobook",
			isfull: 0,
			inprocessing: 1
		});
	}

	//Загружаем архив с аудиокнигой
	var path = fileModel.download(params.downloadurl);

	if (downloadHistory == null) {
		var listDownloadHistory = tableModel.findbyparams("downloadhistory", { url: params.url });

		if (listDownloadHistory != null) {
			downloadHistory = listDownloadHistory[0];
		}
	}

	tableModel.update("downloadhistory", {
		recid: downloadHistory.recid,
		recupdated: new Date(),
		isfull: 1,
		inprocessing: 0,
		path: path
	});

	var files = fileModel.unpack(path);

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
			recdescription: params.name,
			mediatype: "audiobook",
			path: file.Path,
			chapter: chapter,
			ownerrecid: ownerrecid,
			audiobookurl: params.url
		};

		return tableModel.insert("tracks", tableParams);
	});
}
