function checkNav(sel) {
	var qs = new Querystring();
	if (qs.contains('q')) qs.remove('q');

	if (sel.name == "db") qs.set('db', getSelectedValue(sel));
	else if (sel.name == "tbl") qs.set('tbl', getSelectedValue(sel));

	window.parent.location = "default.aspx?" + qs.toString();
}
function updateForm(o) {
	if (o.id.substring(0, 2) == "op" && getSelectedValue(o) != "") document.getElementById("use_" + o.id.substring(3)).checked = true
	else if (o.id.substring(0, 2) == "ex" && getSelectedValue(document.getElementById("op_" + o.id.substring(3))) != "") document.getElementById("use_" + o.id.substring(3)).checked = true;
	else document.getElementById("use_" + o.id.substring(3)).checked = false;
}
function getSelectedValue(sel) {
	return sel.childNodes[sel.selectedIndex].value;
}
