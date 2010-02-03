function updateForm(o) {
	if (o.id.substring(0, 2) == "op" && getSelectedValue(o) != "") document.getElementById("use_" + o.id.substring(3)).checked = true
	else if (o.id.substring(0, 2) == "ex" && getSelectedValue(document.getElementById("op_" + o.id.substring(3))) != "") document.getElementById("use_" + o.id.substring(3)).checked = true;
	else document.getElementById("use_" + o.id.substring(3)).checked = false;
}
function getSelectedValue(sel) {
	return sel.childNodes[sel.selectedIndex].value;
}
