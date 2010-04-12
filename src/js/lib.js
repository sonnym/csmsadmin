function getSelectedValue(sel) {
	return sel.childNodes[sel.selectedIndex].value;
}
  /////////////
 // display //
/////////////
function switchTheme() {
	document.theme.submit();
}
  ////////////////
 // navigation //
////////////////
function checkNav(sel) {
	var qs = new Querystring();
	if (qs.contains('q')) qs.remove('q');

	if (sel.name == "db") {
		if (qs.contains('tbl')) qs.remove('tbl');
		qs.set('db', getSelectedValue(sel));
	} else if (sel.name == "tbl") qs.set('tbl', getSelectedValue(sel));

	window.parent.location = "default.aspx?" + qs.toString();
}
  ////////////
 // select //
////////////
function updateSelectForm(o) {
	if (o.id.substring(0, 2) == "op" && getSelectedValue(o) != "") document.getElementById("use_" + o.id.substring(3)).checked = true
	else if (o.id.substring(0, 2) == "ex" && getSelectedValue(document.getElementById("op_" + o.id.substring(3))) != "") document.getElementById("use_" + o.id.substring(3)).checked = true;
	else document.getElementById("use_" + o.id.substring(3)).checked = false;
}
  /////////////
 // restore //
/////////////
var fbwin;
function openFileBrowser() {
	var qs = new Querystring();
	if (!fbwin || fbwin.closed) {
		fbwin = window.open("browse_srv.aspx?a=" + qs.get("a") + "&n=0", "_blank", "location=0,scrollbars=1,status=0,toolbar=0,left=0,top=0,width=400,height=" + $(window).height());
	} else fbwin.focus();
}
function updateRestorePath(val) {
	var qs = new Querystring();
	$.ajax({ url: 'browse_srv.aspx?a=' + qs.get('a') + '&d=' + escape(val), dataType: 'json',
		success: function(data) {
			var tbl = $('#files');
			tbl.find("tr").remove();
			if (data.errors.failures.length > 0 || data.errors.warnings.length > 0) alert('error');
			for (var i = 0, l = data.payload.length; i < l; i++) {
				var folder = data.payload[i].t == 0;
				$('#files > tbody:last').append('<tr class="' + ((i % 2 == 0) ? 'even' : 'odd') + ' pointer" onclick="' + ((folder) ? 'update' : 'set') + 'RestorePath(\'' + data.payload[i].fn  + '\')">' +
													'<td><img src="themes/' + $('#h_theme').val() + '/img/' + ((folder) ? 'folder' : 'file') + '.png" />' + data.payload[i].dn + '</td></tr>');
			}
		}
	});
}
function setRestorePath(v) {
	window.opener.$('#file').val(v);
	window.opener.$('#filedisp').text(v);
	window.opener.$('#filewrapper').css('display', 'block');
	window.close();
}
function restoreHeaderOnly() {
	var qs = new Querystring();
	var url = 'restore.aspx?a=' + qs.get('a') + '&fn=rh&f=' + escape($('#file').val());
	$.ajax( {url: url, dataType: 'json',
		success: function(data) {
			if ($('#restoreHeader :not(:first)')) $('#restoreHeader :not(:first)').remove();

			$('#restoreHeader').css({ 'display': 'block' });
			$('#restoreHeader').append('<br /><span>Database Name: ' + data.payload[0].dbn + '</span><br />');
			$('#restoreHeader').append('<span>Database Version: ' + data.payload[0].dbv + '</span><br />');
			$('#restoreHeader').append('<span>Backup Size: ' + data.payload[0].bs + '</span><br />');
			$('#restoreHeader').append('<span>Compatibility Level: ' + data.payload[0].cl + '</span><br />');
			$('#restoreHeader').append('<span>Collation: ' + data.payload[0].col + '</span><br />');
			$('#restoreHeader').append('<span>Read Only: ' + data.payload[0].iro + '</span><br />');
			$('#restoreHeader').append('<span>Damaged: ' + data.payload[0].id + '</span>');
		}
	});
}
function restoreFileListOnly() {
	var qs = new Querystring();
	var url = 'restore.aspx?a=' + qs.get('a') + '&fn=rfl&f=' + escape($('#file').val());
	$.ajax( {url: url, dataType: 'json',
		success: function(data) {
			$('#restoreFileList').css({ 'display': 'block' });
			$('#restoreFileList > table:last > tbody:last').empty();
			$('#restoreFileList > table:last > tbody:last').append('<tr class="title"><td /><td>Logical Name</td><td>Physical Name</td><td>New Physical Name</td></tr>');
			for (var i = 0, l = data.payload.length; i < l; i++) $('#restoreFileList > table:last > tbody:last').append('<tr class="' + ((i % 2 == 0) ? "even" : "odd") + '">' +
																	'<td><input type="checkbox" name="f_' + data.payload[i].fid  + '" /></td><td>' + data.payload[i].ln + '</td>' +
																	'<td>' + data.payload[i].pn + '</td>' +
																	'<td><input type="text" id="pn_' + data.payload[i].fid + '" /></td></tr>');
			$('#restoreFileList > table:last > tbody:last').append('<tr class="' + ((data.payload.length % 2 == 0) ? 'even' : 'odd') + '">' +
																	'<td colspan="4">Physical Name Path: <input type="text" id="pn" size="75" /></td></tr>');
		}
	});
}
function restoreDatabase() {
	var qs = new Querystring();

	var rows = $('#restoreFileList > table:last > tbody:last > tr.odd, tr.even');
	var q = '', ids = '';
	for (var i = 0, l = rows.length - 2; i < l; i++) if (rows[i].children[0].children[0].checked) {
		var pn = $('#pn').val();
		if (pn.length > 0) q += '&pn=' + escape(pn);

		var id = rows[i].children[0].children[0].name.substring(2);
		ids += ((ids.length > 0) ? '%2c' /*,*/ : '') + id;
		q += "&" + 'pn_' + id + '=' + escape($('#pn_' + id).val());
	}

	var url = '?a=' + qs.get('a') + '&fn=rdb&f=' + escape($('#file').val()) + '&dbname=' + escape($('#dbname').val()) + "&ids=" + ids + q;
	$.ajax( {url: url, dataType: 'json',
		success: function(data) {
		}
	});
}
