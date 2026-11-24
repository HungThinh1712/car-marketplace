(function () {
  var urls = [];

  function clearUrls() {
    for (var index = 0; index < urls.length; index += 1) {
      URL.revokeObjectURL(urls[index]);
    }
    urls = [];
  }

  function render(input) {
    var form = input.form || input.closest("form");
    if (!form) {
      return;
    }

    var files = Array.prototype.filter.call(input.files || [], function (file) {
      return file.size > 0;
    }).slice(0, 10);
    var review = form.querySelector(".photo-review");
    clearUrls();

    if (files.length === 0) {
      if (review) {
        review.remove();
      }
      return;
    }

    if (!review) {
      review = document.createElement("div");
      review.className = "photo-review";
      review.setAttribute("aria-label", "Selected photos");
      input.parentElement.insertAdjacentElement("afterend", review);
    }

    review.replaceChildren();
    files.forEach(function (file, index) {
      var figure = document.createElement("figure");
      var image = document.createElement("img");
      var caption = document.createElement("figcaption");
      var url = URL.createObjectURL(file);
      urls.push(url);
      image.src = url;
      image.alt = file.name;
      caption.textContent = index === 0 ? "Cover" : "Photo " + (index + 1);
      figure.append(image, caption);
      review.append(figure);
    });
  }

  document.addEventListener(
    "change",
    function (event) {
      var target = event.target;
      if (!(target instanceof HTMLInputElement) || target.type !== "file" || target.name !== "photos") {
        return;
      }

      window.setTimeout(function () {
        if (target.dataset.reviewed === "react") {
          return;
        }
        render(target);
      }, 0);
    },
    true,
  );
})();
