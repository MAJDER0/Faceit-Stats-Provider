﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var canvas = document.querySelector('#faceitlvl');
var c = canvas.getContext('2d');
const centerX = canvas.width / 2;
const centerY = canvas.height / 2;
const circle = canvas.width / 2;

var x = -3.983;
var dx = 0.10;

function generate10lvl() {

    c.beginPath();
    c.arc(centerX, centerY, circle, 0, 2 * Math.PI);
    c.strokeStyle = "#1F1F22";
    c.fillStyle = "#1F1F22";
    c.fill();
    c.stroke();

    c.beginPath();
    c.arc(centerX, centerY, circle * 0.7, 2.3, x);
    c.lineWidth = circle * 0.2;
    c.strokeStyle = "#FE1F00";
    c.fill();
    c.stroke();

    c.beginPath();
    c.font = circle * 0.70 + "px 'Play', sans-serif";
    c.textBaseline = "middle";
    c.textAlign = "center";
    c.lineWidth = circle * 0.04;
    c.strokeStyle = "#FE1F00";
    c.strokeText("10", centerX - circle * 0.035, centerY);
    c.fillStyle = "#FE1F00";
    c.fillText("10", centerX - circle * 0.035, centerY);
    c.fill();
    c.stroke();

    function animate() {

        c.beginPath();
        c.lineWidth = circle * 0.2;
        c.arc(centerX, centerY, circle * 0.7, 2.3, x);
        c.stroke();

        if (x <= 0.85) {
            x += dx;
            requestAnimationFrame(animate);
        }
    }
    animate();
}