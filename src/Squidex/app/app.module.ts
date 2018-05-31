/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { ApplicationRef, NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { DragulaModule } from 'ng2-dragula';


import { AppComponent } from './app.component';

import {
    AnalyticsIdConfig,
    ApiUrlConfig,
    CurrencyConfig,
    DecimalSeparatorConfig,
    SqxFrameworkModule,
    SqxSharedModule,
    TitlesConfig,
    UserReportConfig
} from './shared';

import { SqxShellModule } from './shell';

import { routing } from './app.routes';

export function configApiUrl() {
    return new ApiUrlConfig(window.location.protocol + '//' + window.location.host + '/');
}

export function configTitles() {
    return new TitlesConfig({}, undefined, 'Squidex Headless CMS');
}

export function configAnalyticsId() {
    return new AnalyticsIdConfig('UA-99989790-2');
}

export function configDecimalSeparator() {
    return  new DecimalSeparatorConfig('.');
}

export function configCurrency() {
    return new CurrencyConfig('EUR', '€', true);
}

export function configUserReport() {
    return new UserReportConfig('221afe63-0ca2-42aa-8efe-188d77964a7f');
}

@NgModule({
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        DndModule.forRoot(),
        HttpClientModule,
        FormsModule,
        CommonModule,
        RouterModule,
        ReactiveFormsModule,
        SqxFrameworkModule.forRoot(),
        SqxSharedModule.forRoot(),
        SqxShellModule,
        routing
    ],
    declarations: [
        AppComponent
    ],
    providers: [
        { provide: AnalyticsIdConfig, useFactory: configAnalyticsId },
        { provide: ApiUrlConfig, useFactory: configApiUrl },
        { provide: CurrencyConfig, useFactory: configCurrency },
        { provide: DecimalSeparatorConfig, useFactory: configDecimalSeparator },
        { provide: TitlesConfig, useFactory: configTitles },
        { provide: UserReportConfig, useFactory: configUserReport }
    ],
    entryComponents: [AppComponent]
})
export class AppModule {
    public ngDoBootstrap(appRef: ApplicationRef) {
        try {
            appRef.bootstrap(AppComponent);
        } catch (e) {
            console.log('Application element not found');
        }
    }
}