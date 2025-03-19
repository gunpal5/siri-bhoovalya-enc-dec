# Siri Bhuvalya Encoding and Decoding Research

## Overview

This repository houses a research effort dedicated to uncovering the encoding and decoding techniques involved in Siri Bhuvalya. This research has numerous practical applications, including:

- Developing methods for creating multi-lingual text embeddings for computational linguistics
- Establishing efficient techniques to encode multi-lingual text into mathematical representations
- Exploring ancient encoding systems and their relevance to modern information theory

## What is Siri Bhuvalya?

Siri Bhuvalya (also spelled Siri Bhoovalaya) is a unique ancient Jain manuscript composed entirely in a numerical script. These numbers correspond to syllables across 718 languages, integrating knowledge from multiple sciences and cultural texts. It was composed by Muni Shri Kumudendu in the 9th century.

Key characteristics of Siri Bhuvalya:

- Each page consists of a 27×27 matrix (called a "chakra") which can be modeled as a 27×27 toroidal grid
- Each cell contains a number between 1-64 (2^6)
- These numbers can be mapped to scripts (alphabets) for ancient Indian languages such as Sanskrit, Prakrit, Kannada, etc.
- The 27×27 toroidal grid can be traversed in arbitrary ways to extract different text contents

## Research Objectives

This repository specifically aims to analyze the methods involved in encoding multilingual text and answer these questions:

1. How can we create an encoder that takes multilingual text and creates simple mathematical representations?
2. Can we use this technique to create a multilingual text embedding model for use in AI?
3. Is the embedded text created as a crossword puzzle-like representation, or do the mathematical representations have semantic relationships?

## Current Implementation

We have implemented:

- An efficient Hamiltonian Path finding algorithm with specific constraints (like 2 consecutive vowels, 2-4 consonants) for traversing or finding all possible paths with which text can be extracted

## Roadmap

Our research plan includes the following steps:

1. **✓ Path Finding Algorithm**: Implement Hamiltonian path finding with linguistic constraints
2. **⟶ Mathematical Formulation**: Identify the mathematical formula for each path, for example using Lagrange polynomial Mod 27
3. **⟶ Text Extraction**: Extract intelligible text from these pathways
4. **⟶ Encoder Development**: Create a full-fledged encoder to generate matrices or chakras like the original text

## Contributing

Contributions to this research project are welcome. Please feel free to open issues or submit pull requests.

## License

This repository is licensed under the GNU General Public License.

## Notable People and Trust

This research acknowledges the work of:

- **Siri Bhuvalya Research and Training