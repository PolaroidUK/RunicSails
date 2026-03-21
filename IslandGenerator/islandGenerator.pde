PImage islandHeightMap; //<>//
PImage islandNormalMap;
float[] heightMap;

int islandWidth = 3200;
int islandHeight = 3200;

char viewState = '1';

void setup() {
  size(1600, 1600);
  generateIsland();
}

void draw() {
  //background(20, 50, 120);
  background(70);

  var image = islandHeightMap;
  switch(viewState) {
  case '1':
    image = islandHeightMap;
    break;
  case '2':
    image = islandNormalMap;
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
    viewState = key;
    break;
  case 'p':
    printIsland();
    break;
  }
}

void printIsland() {
  String baseurl = "island-" + hour()+minute()+second();
  islandHeightMap.save(baseurl + "/base.png");
  islandNormalMap.save(baseurl + "/normal.png");
}

void generateIsland() {
  islandHeightMap = createImage(islandWidth, islandHeight, ARGB);
  islandNormalMap = createImage(islandWidth, islandHeight, ARGB);
  heightMap = new float[islandWidth * islandHeight];

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

    heightMap[i] = noiseValue;

    // if below sea level -> transparent
    if (noiseValue < 0) {
      islandHeightMap.pixels[i] = color(0, 0, 0, 0);
      continue;
    }

    // clamp normalized height to [0,1]
    float h = constrain(noiseValue, 0, 1);

    // apply color banding per height (separate function)
    islandHeightMap.pixels[i] = colorForHeight(h);
  }
}


color colorForHeight(float h) {
  // color bands from deep -> peak
  color[] bands = {
    //color(0, 40, 120),    // deep water
    //color(50, 110, 200),  // shallow water
    color(240, 220, 160), // sand
    color(90, 160, 70),   // grass
    color(110, 100, 90),  // rock
    color(240, 240, 255)  // snow/peak
  };
  int bandCount = bands.length;
  int band = min(floor(h * bandCount), bandCount - 1);
  color c = bands[band];
  
  if(h < 0){
    return color(0,0);
  }
  // ensure fully opaque for land pixels
  return color(red(c), green(c), blue(c), 255);
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
  float c = heightMap[idx];
  if (c < 0) return 0;
  return c;
}
