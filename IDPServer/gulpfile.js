const gulp = require('gulp');
const uglify = require('gulp-uglify');
const cleanCSS = require('gulp-clean-css');
const rename = require('gulp-rename');

// Minify JavaScript
gulp.task('scripts', function () {
    return gulp.src('wwwroot/lib/**/*.js')
        .pipe(uglify())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('wwwroot/lib'));
});

// Minify CSS
gulp.task('styles', function () {
    return gulp.src('wwwroot/css/*.css')
        .pipe(cleanCSS())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('wwwroot/css'));
});

// Default task - run only minification tasks
gulp.task('default', gulp.series('scripts', 'styles'));
