import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";
import tsdoc from "eslint-plugin-tsdoc";
import jsdoc from "eslint-plugin-jsdoc";

export default tseslint.config(
  { ignores: ["dist", "node_modules"] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ["**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2022,
      globals: { ...globals.browser },
    },
    plugins: {
      "react-hooks": reactHooks,
      "react-refresh": reactRefresh,
      "tsdoc": tsdoc,
      "jsdoc": jsdoc,
    },
    rules: {
      ...reactHooks.configs.recommended.rules,
      "react-refresh/only-export-components": ["warn", { allowConstantExport: true }],
      "tsdoc/syntax": "error",
      "jsdoc/require-jsdoc": [
        "error",
        {
          publicOnly: true,
          require: {
            FunctionDeclaration: true,
            MethodDefinition: true,
            ClassDeclaration: true,
            ArrowFunctionExpression: true,
            FunctionExpression: true,
          },
          contexts: [
            "VariableDeclaration > VariableDeclarator > ArrowFunctionExpression",
            "VariableDeclaration > VariableDeclarator > FunctionExpression",
            "ExportNamedDeclaration > VariableDeclaration > VariableDeclarator > ArrowFunctionExpression",
            "ExportNamedDeclaration > VariableDeclaration > VariableDeclarator > FunctionExpression",
          ],
        },
      ],
      "jsdoc/require-description": "error",
      "jsdoc/require-param": "error",
      "jsdoc/require-returns": "error",
    },
  }
);
