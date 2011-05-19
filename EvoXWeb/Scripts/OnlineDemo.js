onload = function () {
    if (document.getElementsByClassName == undefined) {
        document.getElementsByClassName = function (className) {
            var hasClassName = new RegExp("(?:^|\\s)" + className + "(?:$|\\s)");
            var allElements = document.getElementsByTagName("*");
            var results = [];

            var element;
            for (var i = 0; (element = allElements[i]) != null; i++) {
                var elementClass = element.className;
                if (elementClass && elementClass.indexOf(className) != -1 && hasClassName.test(elementClass))
                    results.push(element);
            }

            return results;
        }
    }
}

function selectComponent(elementId) {
    selected = document.getElementsByClassName("selected");
    
    for (key in selected) {
        selected[key].className = null;
    }
    
    var element = document.getElementById(elementId);
    if (element != null) {
        element.setAttribute("class", "selected");
    }
}

function selectComponents(elements) {
    selected = document.getElementsByClassName("selected");

    for (key in selected) {
        selected[key].className = null;
    }

    for (key in elements) { 
        var element = document.getElementById(elements[key]);
        if (element != null) {
            element
        }
    }
}

function highlightTokenUsage(token) {
    selected = document.getElementsByClassName("selectedToken");
    for (key in selected) {
		if (key != "item" && key != "length" && key != "namedItem")
		{
			newValue = selected[key].getAttribute("class").replace("selectedToken", "").replace(" ", "");
			selected[key].setAttribute("class", newValue);
		}
    }

    toSelect = document.getElementsByClassName(token);

    //alert(document.write(selected.length))

    for (key in toSelect) {
        if (key != "item" && key != "length" && key != "namedItem")
		{
			toSelect[key].setAttribute("class", token + " selectedToken");
		}
    }
}

function FocusPIMTab() {
    FocusTab(2);
}

function FocusTab(index) {
    var tabContainer = document.getElementById('MainContent_tcProject');
    if (tabContainer != undefined && tabContainer != null) {
        tabContainer = tabContainer.control;
        tabContainer.set_activeTabIndex(index);
    }
}