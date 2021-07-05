#!/bin/bash
asciidoctor -D assets AspNetCoreHun.adoc
asciidoctor-pdf -D assets AspNetCoreHun.adoc
asciidoctor-epub3 -D assets AspNetCoreHun.adoc