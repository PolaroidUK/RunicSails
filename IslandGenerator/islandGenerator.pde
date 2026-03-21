PImage islandHeightMap; //<>//
PImage islandNormalMap;
PImage islandBitMap;

int islandWidth = 3200;
int islandHeight = 3200;

char viewState = '1';

void setup() {
  size(1600, 1600);
  generateIsland();
}

void draw() {
  background(20, 50, 120);

  var image = islandHeightMap;
  switch(viewState) {
  case '1':
    image = islandHeightMap;
    break;
  case '2':
    image = islandNormalMap;
    break;
  case '3':
    image = islandBitMap;
    break;
  }
  image(image, 0, 0, width, height);
}

void keyPressed() {
  switch(key) {
  case 'r':
    generateIsland();
    break;
  case '1':
  case '2':
  case '3':
    viewState = key;
    break;
  case 'p':
    printIsland();
    break;
  }
}

void printIsland() {
  islandHeightMap.save("island-"+hour()+minute()+second()+"/base.png");
  islandNormalMap.save("island-"+hour()+minute()+second()+"/normal.png");
  islandBitMap.save("island-"+hour()+minute()+second()+"/collision.bmp");
}

void generateIsland() {
  islandHeightMap = createImage(islandWidth, islandHeight, ARGB);
  islandNormalMap = createImage(islandWidth, islandHeight, ARGB);
  islandBitMap = createImage(islandWidth, islandHeight, ALPHA);

  noiseDetail(4, 0.5);
  noiseSeed(floor(random(0, 9000000)));
  generateHeightMap();
  generateNormalMap();
}

void generateHeightMap() {
  for (int i = 0; i < islandHeightMap.pixels.length; i++) {
    float x = i % islandWidth;
    float y = floor(i/islandWidth);

    float centerX = islandWidth/2f;
    float centerY = islandHeight/2f;

    float centerDistance = sqrt(pow((centerX-x), 2)+pow((centerY-y), 2))/islandWidth/3f;

    float noiseValue = noise(x/100f, y/100f)*2-1;
    noiseValue = noiseValue - centerDistance*3f;
    noiseValue*= 2;

    color pixel = color(255-noiseValue*255);

    pixel = noiseValue < 0 ? color(0, 0, 0, 0): pixel;
    islandBitMap.pixels[i] = noiseValue < 0 ? 0: color(255);
    islandHeightMap.pixels[i] = pixel;
  }
}

void generateNormalMap() {
  float strength = 2.0; // adjust to increase/decrease normal influence

  for (int i = 0; i < islandNormalMap.pixels.length; i++) {
    int x = i % islandWidth;
    int y = floor(i/islandWidth);

    // helper to sample height, clamped to edges
    float h = getHeightAt(x, y);
    float hLeft = getHeightAt(max(x-1, 0), y);
    float hRight = getHeightAt(min(x+1, islandWidth-1), y);
    float hUp = getHeightAt(x, max(y-1, 0));
    float hDown = getHeightAt(x, min(y+1, islandHeight-1));

    float dx = (hRight - hLeft) * strength;
    float dy = (hDown - hUp) * strength;

    float nz = 1.0;
    float len = sqrt(dx*dx + dy*dy + nz*nz);
    float nx = dx/len;
    float ny = dy/len;
    float nnz = nz/len;

    int r = int((nx * 0.5 + 0.5) * 255);
    int g = int((ny * 0.5 + 0.5) * 255);
    int b = int((nnz * 0.5 + 0.5) * 255);

    int a = (alpha(islandHeightMap.pixels[i]) == 0) ? 0 : 255;

    islandNormalMap.pixels[i] = color(r, g, b, a);
  }
}

float getHeightAt(int sx, int sy) {
  int idx = sy * islandWidth + sx;
  color c = islandHeightMap.pixels[idx];
  if (alpha(c) == 0) return 0;
  return brightness(c) / 255.0;
}
