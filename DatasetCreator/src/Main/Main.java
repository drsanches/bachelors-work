package Main;

        import org.scilab.forge.jlatexmath.*;
        import java.awt.*;
        import java.io.BufferedReader;
        import java.io.FileInputStream;
        import java.io.InputStreamReader;
        import java.util.ArrayList;

        import org.json.*;

public class Main {
    static final String SYMBOLS_FILENAME = "..\\dataset\\symbols.json";
    static final String IMAGES_DIRECTORY = "..\\dataset\\images";
    static final String OUTPUT_FILENAME = "..\\dataset\\dataset-32x32.npz";
    static final String SCRIPT_FILENAME = "images_to_dataset.py";
    static final int WIDTH = 32;
    static final int HEIGHT = 32;
    static final int SQRT_SPACES_COUNT = 5;

    public static void main(String[] args) {
        ArrayList<String> fonts = getFontsList();

        try {
            JSONObject jsonSymbols = readJsonFromFile(SYMBOLS_FILENAME);
            String[] symbols = jsonSymbols.getString("Symbols").split(" ");

            for (int i = 0; i < symbols.length; i++) {
                if (symbols[i].equals("\\sqrt")) {
                    String spaces = "";
                    for (int k = 0; k < SQRT_SPACES_COUNT; k++) {
                        spaces += "\\ ";
                        for (int j = 0; j < fonts.size(); j++)
                            createSymbolImage(symbols[i] + "{" + spaces + "}", fonts.get(j),
                                    IMAGES_DIRECTORY + "\\" + i + "-" + String.valueOf(j * fonts.size() + k) + ".png");
                    }
                }
                else
                    for (int j = 0; j < fonts.size(); j++)
                        createSymbolImage(symbols[i], fonts.get(j),IMAGES_DIRECTORY + "\\" + i + "-" + j + ".png");

                System.out.println(symbols[i]);
            }
            imagesToDataset(SCRIPT_FILENAME, SYMBOLS_FILENAME, IMAGES_DIRECTORY, WIDTH, HEIGHT, OUTPUT_FILENAME);

        } catch (Exception e) {
            System.out.println("Error: " + e.getMessage());
        }

        System.out.println("Done");
    }

    private static ArrayList<String> getFontsList() {
        ArrayList<String> fonts = new ArrayList<String>();
//        fonts.add("\\mathbb");
//        fonts.add("\\mathfrak");
        fonts.add("\\mathbf");
        fonts.add("\\mathsf");
//        fonts.add("\\mathscr");
        fonts.add("\\mathtt");
        fonts.add("\\mathit");
        fonts.add("\\mathrm");

        return fonts;
    }

    private static JSONObject readJsonFromFile(String filename) throws Exception {
        String fileContent = "";
        BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(filename)));
        String nextString;
        while ((nextString = reader.readLine()) != null)
            fileContent += nextString;
        reader.close();

        return new JSONObject(fileContent);
    }

    private static void createSymbolImage(String symbol, String font, String filename) {
        TeXFormula teXFormula = new TeXFormula(font + "{" + symbol + "}");
        teXFormula.createPNG(TeXConstants.STYLE_DISPLAY, 50, filename, Color.white, Color.black);
    }

    private static void imagesToDataset(String scriptFilename, String filename, String directory,
                                        int width, int height, String outputFilename) throws Exception {
        Process process = new ProcessBuilder()
                .command("python.exe", scriptFilename, filename, directory,
                        String.valueOf(width), String.valueOf(height), outputFilename)
                .start();
        System.out.println(process.waitFor());
    }
}
