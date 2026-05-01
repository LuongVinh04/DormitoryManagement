const fs = require('fs');
let content = fs.readFileSync('msbuild.log', 'utf8');
if (content.includes('\0')) {
  content = fs.readFileSync('msbuild.log', 'utf16le');
}
const lines = content.split(/\r?\n/);
const errors = lines.filter((line) => line.includes('error CS'));
fs.writeFileSync('errors.txt', errors.join('\n'), 'utf8');
