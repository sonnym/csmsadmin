function checkNav(sel) {
	var qs = new Querystring();
	if (qs.contains('q')) qs.remove('q');

	if (sel.name == "db") {
		if (qs.contains('tbl')) qs.remove('tbl');
		qs.set('db', getSelectedValue(sel));
	} else if (sel.name == "tbl") qs.set('tbl', getSelectedValue(sel));

	window.parent.location = "default.aspx?" + qs.toString();
}
function switchTheme() {
	document.theme.submit();
}
function updateSelectForm(o) {
	if (o.id.substring(0, 2) == "op" && getSelectedValue(o) != "") document.getElementById("use_" + o.id.substring(3)).checked = true
	else if (o.id.substring(0, 2) == "ex" && getSelectedValue(document.getElementById("op_" + o.id.substring(3))) != "") document.getElementById("use_" + o.id.substring(3)).checked = true;
	else document.getElementById("use_" + o.id.substring(3)).checked = false;
}
/* need to test before switching
function updateSelectForm(o) {
	if (o.id.substring(0, 2) == "op" && getSelectedValue(o) != "") $("#use_" + o.id.substring(3)).checked = true
	else if (o.id.substring(0, 2) == "ex" && getSelectedValue($("#op_" + o.id.substring(3))) != "") $(#"use_" + o.id.substring(3)).checked = true;
	else $("#use_" + o.id.substring(3)).checked = false;
}
*/
function getSelectedValue(sel) {
	return sel.childNodes[sel.selectedIndex].value;
}
var fbwin;
function openFileBrowser() {
	var qs = new Querystring();
	if (!fbwin || fbwin.closed) {
		fbwin = window.open("browse_srv.aspx?a=" + qs.get("a") + "&n=0", "_blank", "location=0,scrollbars=0,status=0,toolbar=0,left=0,top=0,width=400,height=400");
	} else fbwin.focus();
}
function updateRestorePath(val) {
	var qs = new Querystring();
	$.ajax({url: 'browse_srv.aspx?a=' + qs.get('a') + '&d=' + escape(val),
			dataType: 'json',
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
